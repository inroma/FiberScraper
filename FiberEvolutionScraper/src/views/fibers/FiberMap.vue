<script setup lang="ts">
import { ISnackbarColor } from '@/models/SnackbarInterface';
import type FiberPointDTO from '@/models/FiberPointDTO';
import { EtapeFtth } from '@/models/Enums';
import { fiberService } from '@/services/FiberService';
import { useToastStore } from '@/store/ToastStore';
import * as extent from 'ol/extent';
import type { OlFeature } from "vue3-openlayers/map";
import type { OlVectorLayer } from "vue3-openlayers/layers";
import type { OlSourceVector } from "vue3-openlayers/sources";
import type { OlGeomPoint } from "vue3-openlayers/geometries";
import type { OlInteractionSelect } from "vue3-openlayers/interactions";
import type { OlStyle, OlStyleIcon } from "vue3-openlayers/styles";
import { MapConstants } from '@/shared/constants/MapConstants';
import dayjs from 'dayjs';
import { useDisplay, useTheme } from 'vuetify';
import type { SelectEvent } from 'ol/interaction/Select';
import { useMapStore } from '@/store/mapStore';
import { storeToRefs } from 'pinia';
import type { AxiosPromise } from 'axios';
import MinMaxBounds from '@/models/MinMaxBounds';

//#region Public Properties
const loading = ref(false);
const controlOpened = ref(false);
const fibers: Ref<FiberPointDTO[]> = ref([]);
const selectedFiber = ref<FiberPointDTO>(null);
const selectedPosition = ref<number[]>();
const date = dayjs();
const recentResult = ref(false);
const resultFromDb = ref(false);
const layers = ref<{ markers: FiberPointDTO[], name: string, visible: boolean }[]>(
	Array.from(Object.values(EtapeFtth).filter(a => typeof(a) === 'string').map(l => ({ markers: [] as FiberPointDTO[], name: l, visible: true  })))
);
const icons = [{code: EtapeFtth[EtapeFtth.ELLIGIBLE_XGSPON], title: "Éligible 10Gb/s", icon: MapConstants.greenestIcon, order: 21},
	{code: EtapeFtth[EtapeFtth.ELIGIBLE], title: "Éligible", icon: MapConstants.greenIcon, order: 20},
	{code: EtapeFtth[EtapeFtth.PROCHE_CLIENT], title: "Proche Client", icon: MapConstants.yellowIcon, order: 19},
	{code: EtapeFtth[EtapeFtth.EN_COURS_IMMEUBLE], title: "Déploiement Immeuble", icon: MapConstants.purpleIcon, order: 18},
	{code: EtapeFtth[EtapeFtth.TERMINE_QUARTIER], title: "Quartier Terminé", icon: MapConstants.blueInvertedIcon, order: 17},
	{code: EtapeFtth[EtapeFtth.EN_COURS_QUARTIER], title: "Déploiement Quartier", icon: MapConstants.defaultIcon, order: 16},
	{code: EtapeFtth[EtapeFtth.PREVU_QUARTIER], title: "Quartier Programmé", icon: MapConstants.brownIcon, order: 15},
	{code: EtapeFtth[EtapeFtth.NON_PREVU], title: "Non prévu", icon: MapConstants.pinkIcon, order: 14},
	{code: EtapeFtth[EtapeFtth._], title: "Aucune donnée", icon: MapConstants.redIcon, order: 13},
	{code: EtapeFtth[EtapeFtth.UNKNOWN], title: "Statut inconnu", icon: MapConstants.blackIcon, order: 2}
];
const { view } = storeToRefs(useMapStore());
const { mobile, mdAndDown, mdAndUp } = useDisplay();
const { current } = useTheme();
const isDarkTheme = computed(() => current.value.dark)

const headers = [
	{ title: 'Adresse', value: 'libAdresse' },
	{ title: 'Création', value: 'created', sortable: true },
	{ title: 'Batiment', value: 'batiment', width: '250px' },
	{ title: 'Éligibilité FTTH', value: 'eligibilitesFtth' },
	{ title: 'Coord Lat', value: 'x', sortable: true },
	{ title: 'Coord Long', value: 'y', sortable: true },
];

const toastStore = useToastStore();

//#endregion

function getCloseAreaFibers() {
	recentResult.value = resultFromDb.value = false;
	loading.value = true;
	var promise = fiberService.getCloseAreaFibers(view.value.getCenter());
	handleApiResponse(promise);
}

function updateFibers() {
	loading.value = true;
	fiberService.updateWideArea(view.value.getCenter())
	.then((response) => {
		toastStore.createToastMessage({ message: response.data + " points créés / mis à jour." });
	})
	.catch((errors) => {
		toastStore.createToastMessage({ color: ISnackbarColor.Error, message: errors });
	})
	.finally(() => loading.value = false);
}

function getFibers() {
	recentResult.value = resultFromDb.value = false;
	loading.value = true;
	var promise = fiberService.getWideArea(view.value.getCenter());
	handleApiResponse(promise);
}

function getDbFibers() {
	resultFromDb.value = loading.value = true;
	recentResult.value = false;
	var data = new MinMaxBounds(view.value?.calculateExtent());
	var promise = fiberService.getDbFibers(data);
	handleApiResponse(promise);
}

function getNewestFibers() {
	recentResult.value = resultFromDb.value = loading.value = true;
	var data = new MinMaxBounds(view.value?.calculateExtent());
	var promise = fiberService.getNewestPoints(data);
	handleApiResponse(promise);
}

function handleApiResponse(response: AxiosPromise<FiberPointDTO[]>) {
	response.then((response) => {
		fibers.value = response.data;
		mapFibersToLayer();
	})
	.catch((errors) => {
		toastStore.createToastMessage({ color: ISnackbarColor.Error, message: errors });
	})
	.finally(() => {
		loading.value = false;
	});
}

function centerMapOnPoint(_: PointerEvent, row: any) {
	const position = [row.item.x, row.item.y];
	view.value?.setZoom(17);
	view.value?.setCenter(position);
	selectedFiber.value = row.item;
	selectedPosition.value = position;
}

function setIcon(fiber: FiberPointDTO) {
	if (fiber.eligibilitesFtth?.length > 0) {
		let etape = EtapeFtth[fiber.eligibilitesFtth[0].etapeFtth];
		if (etape === EtapeFtth[EtapeFtth.ELLIGIBLE_PIF_XGSPON]) {
			etape = EtapeFtth[EtapeFtth.ELLIGIBLE_XGSPON];
		}
		const icon = icons.filter(a => a.code === etape)[0]?.icon;
		if (icon === undefined) {
			fiber.iconUrl = MapConstants.blackIcon;
		} else {
			fiber.iconUrl = icon;
		}
	} else {
		fiber.iconUrl = MapConstants.redIcon;
	}
	fiber.opacity = opacityWithElderness(fiber);
}

function mapFibersToLayer() {
	fibers.value.forEach(fiber => setIcon(fiber));
	Object.values(EtapeFtth).filter(a => typeof(a) === 'string').forEach((value: string) => {
		if (EtapeFtth[value as keyof typeof EtapeFtth] === EtapeFtth.ELLIGIBLE_PIF_XGSPON) {
			return;
		}
		let markers = [];
		// Group all XGSPON together
		if (EtapeFtth[value as keyof typeof EtapeFtth] === EtapeFtth.ELLIGIBLE_XGSPON) {
			markers = fibers.value.filter(f => f.eligibilitesFtth.at(0)?.etapeFtth === EtapeFtth.ELLIGIBLE_XGSPON
				|| f.eligibilitesFtth.at(0)?.etapeFtth === EtapeFtth.ELLIGIBLE_PIF_XGSPON);
		} else {
			markers = fibers.value.filter(f => EtapeFtth[(f.eligibilitesFtth.at(0)?.etapeFtth ?? EtapeFtth._)] === value);
		}
		layers.value.find(l => l.name === value).markers = markers;
	});
	const debugLayer = layers.value.filter(l => l.name === EtapeFtth[EtapeFtth.UNKNOWN])[0];
	debugLayer.markers = debugLayer.markers?.concat(
		fibers.value.filter(f => f.eligibilitesFtth?.some(x => x.etapeFtth === EtapeFtth.DEBUG))
	);
}

function opacityWithElderness(fiber: FiberPointDTO) {
	if (!recentResult.value) {
		return 1;
	}
	let mostRecentDate = dayjs.max(...fiber.eligibilitesFtth.map(f => dayjs(f.lastUpdated)));
	if (fiber.eligibilitesFtth.length === 0) {
		mostRecentDate = dayjs(fiber.lastUpdated);
	}
	let result = 0.2;
	if (mostRecentDate >= date.add(-1, 'day')) {
		result = 1;
	} else if (mostRecentDate >= date.add(-2, 'day')) {
		result = 0.7;
	} else if (mostRecentDate >= date.add(-3, 'day')) {
		result = 0.55;
	}
	return result;
}

function featureSelected(event: SelectEvent) {
	if (event.selected.length == 1) {
		selectedPosition.value = extent.getCenter(
			event.selected.at(0).getGeometry().getExtent(),
		);
		selectedFiber.value = event.selected.at(0).get("fiber");
	} else {
		selectedFiber.value = null;
	}
};

</script>
<template>
	<VCard key="main-card" :class="mobile ? 'mx-0' : 'mx-10'">
		<template #title>
			<VCardActions ref="menu" class="d-inline-flex justify-center">
				<HeaderButtonsComponent v-if="mdAndUp" class="py-0" :loading @updateFibers="updateFibers" @getDbFibers="getDbFibers"
					@getFibers="getFibers" @getCloseAreaFibers="getCloseAreaFibers" @getNewestFibers="getNewestFibers"/>

				<VBtn v-else color="primary" variant="flat">Actions
					<VMenu activator="parent" location="bottom center">
						<VSheet style="width: min-content;">
							<HeaderButtonsComponent :loading @updateFibers="updateFibers" @getDbFibers="getDbFibers"
								@getFibers="getFibers" @getCloseAreaFibers="getCloseAreaFibers" @getNewestFibers="getNewestFibers"/>
						</VSheet>
					</VMenu>
				</VBtn>
			</VCardActions>
		</template>
		<VRow justify="center">
			<VCol lg="5" md="6" sm="10">
				<AddressLookup />
			</VCol>
		</VRow>
		<MapComponent :loading :popup-position="selectedPosition" :show-popup="!!selectedPosition" @click="controlOpened = false">
			<template #vectorLayers>
				<OlVectorLayer v-for="layer in layers.filter((l: any) => l.markers.length && icons.map((i: any) => i.code).find((a: string) => a === l.name))" 
								:title="icons.find((i: any) => i.code === layer.name).title" :key="'layer_'+layer.name" :preview="layer.markers.at(0)?.iconUrl">
					<OlSourceVector>
						<OlFeature v-for="item in layer.markers" :key="'marker_'+item.signature" :properties="{ fiber: item, icon: item.iconUrl }">
							<OlGeomPoint :coordinates="[item.x, item.y]"/>
							<OlStyle>
								<OlStyleIcon :src="item.iconUrl" :anchor="[12.5, 0]" :opacity="item.opacity"
									anchor-x-units="pixels" anchor-y-units="pixels" :scale="1" anchor-origin="bottom-left"/>
							</OlStyle>
						</OlFeature>
					</OlSourceVector>
				</OlVectorLayer>
				<OlInteractionSelect @select="featureSelected">
					<OlStyle>
						<OlStyleIcon v-if="selectedFiber" :src="selectedFiber.iconUrl" :anchor="[12.5, 0]"
								anchor-x-units="pixels" anchor-y-units="pixels" :scale="1" anchor-origin="bottom-left"/>
					</OlStyle>
				</OlInteractionSelect>
			</template>
			<template #popup>
				<MapPopupComponent :fiber="selectedFiber" :resultFromDb />
			</template>
			<template #ui>
				<div class="overlay bottom" @click.stop>
					<div v-if="mdAndDown && !controlOpened" :class="['custom-control', { 'custom-control-dark': isDarkTheme }]">
						<VBtn size="small" variant="text" :ripple="false" @click.stop="controlOpened = true">
							<VIcon size="22" :color="isDarkTheme ? 'white' : 'black'" icon="mdi-map-marker-multiple-outline" />
						</VBtn>
					</div>
					<div v-else :class="['custom-control', { 'custom-control-dark': isDarkTheme }]">
						<VBtn style="z-index:2;" class="my-1 mx-2" size="comfortable" readonly
									v-for="icon in icons" :key="'control-custom-btn-'+icon.code" variant="text">
							<VImg height="27" width="20" :src="icon.icon"/>
							<p class="pl-2 text-overline">{{ icon.title }}</p>
						</VBtn>
					</div>
				</div>
			</template>
		</MapComponent>

		<div class="ml-10 mr-10 justify-center">
			<VRow key="main-card-content" justify="center">
				<VCol md="10">
					<VDataTable class="mt-5 mb-10" :headers="headers" key="list-details" :items="fibers" fixed-header height="650px"
							:mobile-breakpoint="'md'" :mobile='null'
							itemsPerPage="100" :loading="loading" @click:row="centerMapOnPoint" :items-per-page-options="[50, 100, 200, 500, 1000]">
						<template #item.eligibilitesFtth="{ item }">
							<td key="list-item-eligibilite">{{ EtapeFtth[item.eligibilitesFtth[0]?.etapeFtth] }}</td>
						</template>
						<template #item.batiment="{ item }">
							<td key="list-item-batiment">{{ item.eligibilitesFtth[0]?.batiment }}</td>
						</template>
						<template #item.created="{ item }">
							<td key="list-item-created">{{ new Date(item.eligibilitesFtth[0]?.created ?? item.created).toLocaleString() }}</td>
						</template>
					</VDataTable>
				</VCol>
			</VRow>
		</div>
	</VCard>
</template>
<style src="@/assets/css/main.css" />