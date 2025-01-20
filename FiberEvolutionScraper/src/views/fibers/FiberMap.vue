<script setup lang="ts">
import { ISnackbarColor } from '@/models/SnackbarInterface';
import type FiberPointDTO from '@/models/FiberPointDTO';
import { EtapeFtth } from '@/models/Enums';
import FiberService from '@/services/FiberService';
import { useToastStore } from '@/store/ToastStore';
import { Map, Layers, Sources, Styles, Geometries, Interactions } from 'vue3-openlayers';
import { MapHelper } from '@/helpers/MapHelper';
import dayjs from 'dayjs';
import { useDisplay, useTheme } from 'vuetify';
import type { SelectEvent } from 'ol/interaction/Select';
import { useMapStore } from '@/store/mapStore';
import { storeToRefs } from 'pinia';

//#region Public Properties
const loading = ref(false);
const controlOpened = ref(false);
const fibers: Ref<FiberPointDTO[]> = ref([]);
const selectedFiber = ref<FiberPointDTO>(null);
const selectedPosition = ref<number[]>();
const date = dayjs();
const recentResult = ref(false);
const resultFromDb = ref(false);
const extent = inject("ol-extent");
const layers = ref<{ markers: FiberPointDTO[], name: string, visible: boolean }[]>(
    Array.from(Object.values(EtapeFtth).filter(a => typeof(a) === 'string').map(l => ({ markers: [] as FiberPointDTO[], name: l, visible: true  })))
);
const icons = [{code: EtapeFtth[EtapeFtth.ELLIGIBLE_XGSPON], title: "Éligible 10Gb/s", icon: MapHelper.greenestIcon, order: 21},
    {code: EtapeFtth[EtapeFtth.ELIGIBLE], title: "Éligible", icon: MapHelper.greenIcon, order: 20},
    {code: EtapeFtth[EtapeFtth.PROCHE_CLIENT], title: "Proche Client", icon: MapHelper.yellowIcon, order: 19},
    {code: EtapeFtth[EtapeFtth.EN_COURS_IMMEUBLE], title: "Déploiement Immeuble", icon: MapHelper.purpleIcon, order: 18},
    {code: EtapeFtth[EtapeFtth.TERMINE_QUARTIER], title: "Quartier Terminé", icon: MapHelper.blueInvertedIcon, order: 17},
    {code: EtapeFtth[EtapeFtth.EN_COURS_QUARTIER], title: "Déploiement Quartier", icon: MapHelper.defaultIcon, order: 16},
    {code: EtapeFtth[EtapeFtth.PREVU_QUARTIER], title: "Quartier Programmé", icon: MapHelper.brownIcon, order: 15},
    {code: EtapeFtth[EtapeFtth.NON_PREVU], title: "Non prévu", icon: MapHelper.pinkIcon, order: 14},
    {code: EtapeFtth[EtapeFtth._], title: "Aucune donnée", icon: MapHelper.redIcon, order: 13},
    {code: EtapeFtth[EtapeFtth.UNKNOWN], title: "Statut inconnu", icon: MapHelper.blackIcon, order: 2}
];
const { view } = storeToRefs(useMapStore());
const { mobile } = useDisplay();
const { mdAndDown } = useDisplay();
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
    FiberService.getCloseAreaFibers(view.value.getCenter())
    .then((response) => {
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

function updateFibers() {
    loading.value = true;
    FiberService.updateWideArea(view.value.getCenter())
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
    FiberService.getWideArea(view.value.getCenter())
    .then((response) => {
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

function getDbFibers() {
    resultFromDb.value = loading.value = true;
    recentResult.value = false;
    FiberService.getDbFibers(view.value.getCenter())
    .then((response) => {
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

function getNewestFibers() {
    recentResult.value = resultFromDb.value = loading.value = true;
    FiberService.getNewestPoints(view.value?.calculateExtent().join(','))
    .then((response) => {
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
            fiber.iconUrl = MapHelper.blackIcon;
        }
        fiber.iconUrl = icon;
    } else {
        fiber.iconUrl = MapHelper.redIcon;
    }
    fiber.opacity = opacityWithElderness(fiber);
}

function clearFibers() {
    fibers.value = [];
    layers.value.forEach(l => l.markers = []);
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

function centerMapOnLocation() {
    navigator.geolocation.getCurrentPosition(
        position => {
            view.value.setZoom(15);
            view.value.setCenter([position.coords.longitude, position.coords.latitude]);
        }, 
        error => toastStore.createToastMessage({ message: error.message, color: ISnackbarColor.Error }),
        {
            enableHighAccuracy: true,
            timeout: 10000
        });
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
            <VCardActions ref="menu">
                <VBtn icon @click="centerMapOnLocation()" color="primary" class="ml-5">
                    <VIcon>mdi-crosshairs-gps</VIcon>
                </VBtn>
                <HeaderButtonsComponent class="hidden-sm-and-down py-0" :loading="loading" @updateFibers="updateFibers" @getDbFibers="getDbFibers"
                @getFibers="getFibers" @getCloseAreaFibers="getCloseAreaFibers" @getNewestFibers="getNewestFibers"/>
                
                <VBtn class="hidden-md-and-up mx-auto" color="primary" variant="flat">Actions
                    <VMenu activator="parent" offset-y :close-on-content-click="false" offset="0 130">
                        <VList class="hidden-md-and-up">
                            <VListItem>
                                <VRow no-gutters @click="$emit('close')" style="width: min-content;">
                                    <VCol cols="6">
                                        <HeaderButtonsComponent class="d-flex" :loading="loading" @updateFibers="updateFibers" @getDbFibers="getDbFibers"
                                        @getFibers="getFibers" @getCloseAreaFibers="getCloseAreaFibers" @getNewestFibers="getNewestFibers"/>
                                    </VCol>
                                </VRow>
                            </VListItem>
                        </VList>
                    </VMenu>
                </VBtn>
                <VBtn class="mr-5" @click="clearFibers()" color="error" text="Clear" :disabled="!fibers.length" :variant="!fibers.length ? 'outlined' : 'flat'" />
            </VCardActions>
        </template>
        <VRow justify="center">
            <VCol lg="4">
                <AddressLookup />
            </VCol>
        </VRow>
        <MapComponent :loading="loading" :popup-position="selectedPosition" :show-popup="!!selectedPosition" @click="controlOpened = false">
            <template #vectorLayers>
                <Layers.OlVectorLayer v-for="layer in layers.filter(l => l.markers.length && icons.map(i => i.code).find(a => a === l.name))" 
                    :title="icons.find(i => i.code === layer.name).title" :key="'layer_'+layer.name" :preview="layer.markers.at(0)?.iconUrl">
                    <Sources.OlSourceVector>
                        <Map.OlFeature v-for="item in layer.markers" :key="'marker_'+item.signature" :properties="{ fiber: item, icon: item.iconUrl }">
                            <Geometries.OlGeomPoint :coordinates="[item.x, item.y]"/>
                            <Styles.OlStyle>
                                <Styles.OlStyleIcon :src="item.iconUrl" :anchor="[12.5, 0]" :opacity="item.opacity"
                                    anchor-x-units="pixels" anchor-y-units="pixels" :scale="1" anchor-origin="bottom-left"/>
                            </Styles.OlStyle>
                        </Map.OlFeature>
                    </Sources.OlSourceVector>
                </Layers.OlVectorLayer>
                <Interactions.OlInteractionSelect @select="featureSelected">
                    <Styles.OlStyle>
                        <Styles.OlStyleIcon v-if="selectedFiber" :src="selectedFiber.iconUrl" :anchor="[12.5, 0]"
                            anchor-x-units="pixels" anchor-y-units="pixels" :scale="1" anchor-origin="bottom-left"/>
                    </Styles.OlStyle>
                </Interactions.OlInteractionSelect>
            </template>
            <template #popup>
                <MapPopupComponent :fiber="selectedFiber" :resultFromDb="resultFromDb" />
            </template>
            <template #ui>
                <div class="overlay bottom" @click.stop>
                    <div v-if="mdAndDown && !controlOpened" :class="['custom-control', { 'custom-control-dark': isDarkTheme }]">
                        <VBtn size="small" variant="text" :ripple="false" @click.stop="controlOpened = true">
                            <VIcon size="22" color="white" icon="mdi-map-marker-multiple-outline" />
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
                            <td key="list-item-eligibilite">
                                {{ EtapeFtth[item.eligibilitesFtth[0]?.etapeFtth] }}
                            </td>
                        </template>
                        <template #item.batiment="{ item }">
                            <td key="list-item-batiment">
                                {{ item.eligibilitesFtth[0]?.batiment }}
                            </td>
                        </template>
                        <template #item.created="{ item }">
                            <td key="list-item-created">
                                {{ new Date(item.eligibilitesFtth[0]?.created ?? item.created).toLocaleString() }}
                            </td>
                        </template>
                    </VDataTable>
                </VCol>
            </VRow>
        </div>
    </VCard>
</template>
<style src="@/assets/css/main.css" />