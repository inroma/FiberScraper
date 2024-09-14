<script setup lang="ts">
import { Ref, computed, onMounted, ref } from 'vue';
import { ISnackbarColor } from '@/models/SnackbarInterface';
import FiberPointDTO from '@/models/FiberPointDTO';
import { EtapeFtth } from '@/models/Enums';
import MapPopupComponent from '@/components/MapPopupComponent.vue';
import FiberService from '@/services/FiberService';
import HeaderButtonsComponent from '@/components/HeaderButtonsComponent.vue';
import { useToastStore } from '@/store/ToastStore';
import { useDisplay, useTheme } from 'vuetify/lib/framework.mjs';
import { View } from 'ol';
import { Map as OlMap, Layers, Sources, MapControls, Styles, Geometries } from 'vue3-openlayers';
import { MapHelper } from '@/helpers/MapHelper';
import type Map from "ol/Map";
import dayjs from 'dayjs';

//#region Public Properties
const loading = ref(false);
const controlOpened = ref(false);
const loadingHistory = ref(false);
const map = ref<{ map: Map }>(null);
const view = ref<View>();
const userLocation = ref<Array<number>>([4.83, 45.76]);
let mapCenter: number[] = userLocation.value;
const fibers: Ref<FiberPointDTO[]> = ref([]);
const zoom = ref(11);
const tileLayers = MapHelper.getTileLayers();
const bounds = ref<Array<number>>([]);
const selectedFiber = ref<FiberPointDTO>(null);
const date = dayjs();
const recentResult = ref(false);
const resultFromDb = ref(false);
const defaultIcon = '/icons/marker-blue.png';
const redIcon = '/icons/marker-red.png';
const pinkIcon = '/icons/marker-pink.png';
const greenIcon = '/icons/marker-green.png';
const yellowIcon = '/icons/marker-yellow.png';
const purpleIcon = '/icons/marker-purple.png';
const brownIcon = '/icons/marker-orange.png';
const blackIcon = '/icons/marker-black.png';
const blueInvertedIcon = '/icons/marker-white.png';
const layers: Ref<{ markers: FiberPointDTO[], visible: boolean, name: string }[]> = ref([]);
const icons = [{code: EtapeFtth[EtapeFtth.ELIGIBLE], title: "Éligible", icon: greenIcon, order: 20},
    {code: EtapeFtth[EtapeFtth.PROCHE_CLIENT], title: "Proche Client", icon: yellowIcon, order: 19},
    {code: EtapeFtth[EtapeFtth.EN_COURS_IMMEUBLE], title: "Déploiement Immeuble", icon: purpleIcon, order: 18},
    {code: EtapeFtth[EtapeFtth.TERMINE_QUARTIER], title: "Quartier Terminé", icon: blueInvertedIcon, order: 17},
    {code: EtapeFtth[EtapeFtth.EN_COURS_QUARTIER], title: "Déploiement Quartier", icon: defaultIcon, order: 16},
    {code: EtapeFtth[EtapeFtth.PREVU_QUARTIER], title: "Quartier Programmé", icon: brownIcon, order: 15},
    {code: EtapeFtth[EtapeFtth.NON_PREVU], title: "Non prévu", icon: pinkIcon, order: 14},
    {code: EtapeFtth[EtapeFtth._], title: "Aucune donnée", icon: redIcon, order: 13},
    {code: EtapeFtth[EtapeFtth.UNKNOWN], title: "Statut inconnu", icon: blackIcon, order: 2}
];

const { mdAndDown } = useDisplay();
const { current } = useTheme();

const headers = [
    { title: 'Adresse', value: 'libAdresse' },
    { title: 'Création', value: 'created', sortable: true },
    { title: 'Batiment', value: 'batiment', width: '250px' },
    { title: 'Éligibilité FTTH', value: 'eligibilitesFtth' },
    { title: 'Coord Lat', value: 'x', sortable: true },
    { title: 'Coord Long', value: 'y', sortable: true },
];

const toastStore = useToastStore();

const isDarkTheme = computed(() => current.value.dark)

//#endregion

onMounted(() => {
    map.value.map.on('click', function (evt) {
        const feature = map.value.map.forEachFeatureAtPixel(evt.pixel, function (feature) {
            return feature;
        });
        if (!feature) {
            selectedFiber.value = null;
            return;
        }
        const fiber = fibers.value.find(f => f.signature === feature.get("signature"));
        selectedFiber.value = fiber;
        getHistorique();
    });
    // centerMapOnLocation();
});

function getCloseAreaFibers() {
    recentResult.value = resultFromDb.value = false;
    loading.value = true;
    layers.value = [];
    FiberService.getCloseAreaFibers(mapCenter.at(0), mapCenter.at(1))
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
    FiberService.updateWideArea(mapCenter.at(0), mapCenter.at(1))
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
    layers.value = [];
    FiberService.getWideArea(mapCenter.at(0), mapCenter.at(1))
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
    layers.value = [];
    FiberService.getDbFibers(mapCenter.at(0), mapCenter.at(1))
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
    recentResult.value = resultFromDb.value = true;
    loading.value = true;
    layers.value = [];
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

function getHistorique() {
    if (!resultFromDb.value) {
        loadingHistory.value = true;
        FiberService.getHistorique(selectedFiber.value.signature)
        .then((response) => {
            selectedFiber.value.created = response.data.created;
            selectedFiber.value.lastUpdated = response.data.lastUpdated;
            selectedFiber.value.eligibilitesFtth = response.data.eligibilitesFtth.sort((a, b) => (a.batiment < b.batiment && a.etapeFtth < b.etapeFtth ? -1 : 1));
        })
        .catch((errors) => {
            toastStore.createToastMessage({ color:ISnackbarColor.Error, message: errors });
        })
        .finally(() => {
            loadingHistory.value = false;
        });
    }
}

function clearData() {
    fibers.value = [];
    layers.value = [];
}

function centerMapOnPoint(_: PointerEvent, row: any) {
    view.value?.setZoom(18);
    view.value?.setCenter([row.item.x, row.item.y]);
}

function setIcon(fiber: FiberPointDTO) {
    if (fiber.eligibilitesFtth.length > 0) {
        const icon = icons.filter(a => a.code === EtapeFtth[fiber.eligibilitesFtth[0].etapeFtth])[0]?.icon;
        if (icon === undefined) {
            fiber.iconUrl = blackIcon;
        }
        fiber.iconUrl = icon;
    } else {
        fiber.iconUrl = redIcon;
    }
    fiber.opacity = opacityWithElderness(fiber);
}

function showHideLayer(code: string) {
    const curr = map.value.map.getAllLayers().find(l => l.get('title') === icons.find(n => n.code === code).title);
    curr.setVisible(!curr.getVisible());
    const selectedLayer = layers.value.filter(l => l.name === code)[0];
    selectedLayer.visible = !selectedLayer.visible;
}

function boundsUpdated() {
    bounds.value = view.value?.calculateExtent().map(x => x);
}

function centerUpdate() {
    mapCenter = view.value?.getCenter();
}

function mapFibersToLayer() {
    fibers.value.forEach(fiber => setIcon(fiber));
    layers.value = [];
    Object.values(EtapeFtth).filter(a => typeof(a) === 'string').forEach((value: string) => {
        layers.value.push(
            {
                markers: fibers.value.filter(f => EtapeFtth[f.eligibilitesFtth[0]?.etapeFtth] === value)?.toReversed(),
                name: value,
                visible: true,
            }
        );
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
    let result = 1;
    if (mostRecentDate >= date.add(-1, 'day')) {
        result = 1;
    } else if (mostRecentDate >= date.add(-2, 'day')) {
        result = 0.7;
    } else if (mostRecentDate >= date.add(-3, 'day')) {
        result = 0.55;
    } else if (mostRecentDate >= date.add(-4, 'day')) {
        result = 0.2;
    }
    return result;
}

const reverseOrderedIcons = computed(() => icons.sort((a, b) => a.order + b.order));

const mapHeight = computed(() => mdAndDown.value ? "50vh" : "75vh");
</script>
<template>
    <VCard key="main-card" class="mx-0 mb-0">
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
                <VBtn class="mr-5" @click="clearData()" color="error" text="Clear" :disabled="!fibers.length" :variant="!fibers.length ? 'outlined' : 'flat'" />
            </VCardActions>
        </template>
        <template #default>
        <div class="ml-10 mr-10 justify-center">
            <VContainer class="pa-0" :width="mdAndDown ? '100%' : '80%'">
            <VLayout>
                <OlMap.OlMap ref="map" :style="{ height:mapHeight, width: '100%' }" @click="controlOpened = false" @moveend="centerUpdate">
                    <OlMap.OlView ref="view" :center="userLocation" :zoom="zoom" :extent="MapHelper.maxBounds" :max-zoom="20" smoothExtentConstraint @change="boundsUpdated"/>
                    <Layers.OlTileLayer v-for="tile, i in tileLayers" :title="tile.name" :visible="tile.visible" :key="'tileLayer_'+i">
                        <Sources.OlSourceOsm :url="tile.url" :attributions="tile.attribution"/>
                    </Layers.OlTileLayer>
                    <MapControls.OlLayerswitcherimageControl mouseover />
                    <Layers.OlVectorLayer v-for="layer in layers.filter(l => icons.map(i => i.code).find(a => a === l.name))" 
                    :title="icons.find(i => i.code === layer.name).title" :key="'layer_'+layer.name">
                        <Sources.OlSourceVector>
                            <OlMap.OlFeature v-for="item in layer.markers" :key="'marker_'+item.signature" :properties="{ signature: item.signature }">
                                <Geometries.OlGeomPoint :coordinates="[item.x, item.y]" />
                                <Styles.OlStyle>
                                    <Styles.OlStyleIcon :src="item.iconUrl" :opacity="item.opacity" anchor-x-units="pixels"
                                    anchor-y-units="pixels" anchor-origin="bottom-left" :anchor="[12.5, 0]"/>
                                </Styles.OlStyle>
                            </OlMap.OlFeature>
                        </Sources.OlSourceVector>
                    </Layers.OlVectorLayer>
                    <VOverlay class="justify-end align-end" persistent model-value contained :scrim="false" no-click-animation>
                        <VExpandTransition>
                            <MapPopupComponent :fiber="selectedFiber" :loading="loadingHistory"/>
                        </VExpandTransition>
                    </VOverlay>
                    <!-- Layers visibility controls -->
                    <div class="overlay bottom">
                        <div v-if="mdAndDown && !controlOpened" :class="['custom-control', { 'custom-control-dark': isDarkTheme }]">
                            <VBtn size="small" variant="text" :ripple="false" @click.stop="controlOpened = true">
                                <VIcon size="24" color="black" icon="mdi-map-marker-multiple-outline" />
                            </VBtn>
                        </div>
                        <div v-else :class="['custom-control', { 'custom-control-dark': isDarkTheme }]">
                            <VBtn style="z-index:2;" class="my-1 mx-2" size="comfortable"
                            v-for="icon in reverseOrderedIcons"
                            :key="'control-custom-btn-'+icon.code" @click="showHideLayer(icon.code)"
                            :variant="layers.find(l => l.name === icon.code)?.visible ? 'text' : 'plain'"
                            :disabled="!layers.find(l => l.name === icon.code)?.markers?.length">
                                <VImg :key="'control-custom-btn-img-'+icon.code" height="27" width="20" :src="icon.icon"/>
                                <p :key="'control-custom-btn-span-'+icon.code" class="pl-2 text-overline">{{ icon.title }}</p>
                            </VBtn>
                        </div>
                    </div>
                </OlMap.OlMap>
            </VLayout>
            </VContainer>
            <VRow key="main-card-content" justify="center">
                <VCol md="10">
                    <VDataTable class="mt-5 mb-10" :headers="headers" key="list-details" :items="fibers" fixed-header height="650px"
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
        </template>
    </VCard>
</template>
<style src="@/assets/css/main.css" />