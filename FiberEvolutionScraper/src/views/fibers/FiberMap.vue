<style>
.leaflet-popup-content {
    min-width: 320px;
}
.custom-control {
  background: rgba(255, 255, 255, 0.785);
  padding-bottom: 0.5em;
  padding-top: 0.5em;
  margin: 0.3em;
  border: 1px solid #aaa;
  border-radius: 0.7em;
  flex-direction: column !important;
  display: flex;
  align-items: flex-start;
  pointer-events: auto;
}
.custom-control-dark {
  background: rgba(90, 90, 90, 0.785) !important;
}
.leaflet-marker-icon {
    margin-left: -12px;
    margin-top: -41px;
    width: 25px;
    height: 41px;
}
.opacity-80 {
    filter:opacity(0.8) !important
}
.opacity-60 {
    filter:opacity(0.6) !important
}
.opacity-50 {
    filter:opacity(0.5) !important
}
.opacity-15 {
    filter:opacity(0.15) !important
}
#mapContainer {
    z-index: 3;
}
</style>

<template>
    <v-card key="main-card" class="mx-0 mb-0">
        <v-card-actions ref="menu">
            <v-btn icon @click="centerMapOnLocation()" color="primary" class="ml-5">
                <v-icon>mdi-crosshairs-gps</v-icon>
            </v-btn>
            <HeaderButtonsComponent class="hidden-sm-and-down" :loading="loading" @updateFibers="updateFibers" @getDbFibers="getDbFibers"
            @getFibers="getFibers" @getCloseAreaFibers="getCloseAreaFibers" @getNewestFibers="getNewestFibers"/>
            
            <v-btn class="hidden-md-and-up mx-auto" color="primary" variant="flat">Actions
            <v-menu activator="parent" offset-y :close-on-content-click="false" offset="0 130">
                <v-list class="hidden-md-and-up">
                    <v-list-item>
                        <v-row no-gutters @click="$emit('close')" style="width: min-content;">
                            <v-col cols="6">
                                <HeaderButtonsComponent class="d-flex" :loading="loading" @updateFibers="updateFibers" @getDbFibers="getDbFibers"
                                @getFibers="getFibers" @getCloseAreaFibers="getCloseAreaFibers" @getNewestFibers="getNewestFibers"/>
                            </v-col>
                        </v-row>
                    </v-list-item>
                </v-list>
            </v-menu>
            </v-btn>
            <v-btn class="mr-5" @click="clearData()" color="error" :disabled="!fibers.length" :variant="!fibers.length ? 'outlined' : 'flat'">Clear</v-btn>
        </v-card-actions>
        <div class="ml-10 mr-10">
            <v-row no-gutters>
                <v-responsive min-width="200">
                    <l-map :style="{ height:mapHeight, width:'100%'}" ref="map" :center="[userLocation.x, userLocation.y]" :zoom="zoom"
                    :max-bounds="maxBounds" @update:center="centerUpdate" @update:bounds="boundsUpdated" @click="controlOpened = false">
                        <!-- Background Tiles Controls -->
                        <l-control-layers position="topright"/>
                        <!-- Background Tiles Providers -->
                        <l-tile-layer v-for="tileProvider of tileProviders"
                            :url="tileProvider.url" :attribution="tileProvider.attribution"
                            :key="tileProvider.name" :name="tileProvider.name" layer-type="base" :visible="tileProvider.visible"
                        />
                        <!-- Map Layers -->
                        <l-layer-group v-for="layer of layers" :visible="layer.visible" :key="'layer_'+layer.name">
                            <l-marker v-for="fiber of layer.markers" :ref="fiber.signature" :key="'layer_'+layer.name+'marker_'+fiber.signature"
                            :lat-lng="[fiber.y, fiber.x]" @click="getHistorique(fiber)">
                                <l-icon :icon-url="fiber.iconUrl" class="leaflet-marker-icon" :class-name="fiber?.iconClassName" :popupAnchor="[0, -41]"/>
                                <l-popup class="ml-n4 mr-n6 mb-n3 mt-6">
                                    <leaflet-popup-content :fiber="fiber" :loading="loadingHistory"/>
                                </l-popup>
                            </l-marker>
                        </l-layer-group>
                        <!-- Layers visibility controls -->
                        <l-control v-if="mdAndDown && !controlOpened" position='bottomleft' :class="['custom-control', { 'custom-control-dark': isDarkTheme }]" disableScrollPropagation>
                            <v-btn small variant="text" :ripple="false" @click.stop="controlOpened = true">
                                <v-icon size="24" color="black">mdi-map-marker-multiple-outline</v-icon>
                            </v-btn>
                        </l-control>
                        <l-control v-else position='bottomleft' :class="['custom-control', { 'custom-control-dark': isDarkTheme }]" disableScrollPropagation>
                            <v-btn style="z-index:2;" class="ma-2" size="comfortable"
                            v-for="icon in orderedIcons"
                            :key="'control-custom-btn-'+icon.code" @click="showHideLayer(icon.code)"
                            :variant="layers[icon.code]?.visible ? 'text' : 'plain'"
                            :disabled="!layers[icon.code] || !layers[icon.code]?.markers?.length">
                                <v-img :key="'control-custom-btn-img-'+icon.code" height="27" width="20" :src="icon.icon"/>
                                <span :key="'control-custom-btn-span-'+icon.code" class="pl-2">{{ icon.title }}</span>
                            </v-btn>
                        </l-control>
                    </l-map>
                </v-responsive>
            </v-row>
            <v-row key="main-card-content" justify="center">
                <v-col md="10">
                    <v-data-table class="mt-5 mb-10" :headers="headers" key="list-details" :items="fibers" fixed-header height="650px"
                        itemsPerPage="100" :loading="loading" @click:row="centerMapOnPoint" :items-per-page-options="[50, 100, 200, 500, 1000]">
                        <template #item.eligibilitesFtth="{ item }">
                            <td key="list-item-eligibilite">
                                {{ item.eligibilitesFtth[0]?.strEtapeFtth }}
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
                    </v-data-table>
                </v-col>
            </v-row>
        </div>
    </v-card>
</template>
<script setup lang="ts">
import 'leaflet/dist/leaflet.css';
import L, { LatLng, Point, latLngBounds } from "leaflet";
import { LMap, LTileLayer, LMarker, LLayerGroup, LPopup, LControl, LControlLayers, LIcon } from "@vue-leaflet/vue-leaflet";
import { Ref, computed, ref } from 'vue';
import { ISnackbarColor } from '@/models/SnackbarInterface';
import FiberPointDTO from '@/models/FiberPointDTO';
import { EtapeFtth } from '@/models/Enums';
import LeafletPopupContent from '@/components/LeafletPopupComponent.vue';
import FiberService from '@/services/FiberService';
import HeaderButtonsComponent from '@/components/HeaderButtonsComponent.vue';
import { useToastStore } from '@/store/ToastStore';
import { useDisplay, useTheme } from 'vuetify/lib/framework.mjs';

//#region Public Properties
const loading = ref(false);
const controlOpened = ref(false);
const loadingHistory = ref(false);
const userLocation = ref(new Point(45.76, 4.83));
const fibers: Ref<FiberPointDTO[]> = ref([]);
const zoom = ref(11);
const tileProviders = [
    {
        name: 'OpenStreetMap',
        visible: true,
        attribution:
        '&copy; <a target="_blank" href="http://osm.org/copyright">OpenStreetMap</a> contributors',
        url: 'https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png'
    },
    {
        name: 'CartoDBDark',
        visible: false,
        url: 'https://{s}.basemaps.cartocdn.com/dark_all/{z}/{x}/{y}{r}.png',
        attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors &copy; <a href="https://carto.com/attributions">CARTO</a>',
        subdomains: 'abcd'
    },
    {
        name: 'OpenTopoMap',
        visible: false,
        url: 'https://{s}.tile.opentopomap.org/{z}/{x}/{y}.png',
        attribution:
        'Map data: &copy; <a href="http://www.openstreetmap.org/copyright">OpenStreetMap</a>, <a href="http://viewfinderpanoramas.org">SRTM</a> | Map style: &copy; <a href="https://opentopomap.org">OpenTopoMap</a> (<a href="https://creativecommons.org/licenses/by-sa/3.0/">CC-BY-SA</a>)',
    }
];
const bounds: Ref<L.LatLngBounds | undefined> = ref(undefined);
const date = new Date();
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
const icons = [{code: EtapeFtth.ELIGIBLE, title: "Éligible", icon: greenIcon, order: 0},
    {code: EtapeFtth.PROCHE_CLIENT, title: "Proche Client", icon: yellowIcon, order: 1},
    {code: EtapeFtth.EN_COURS_IMMEUBLE, title: "Déploiement Immeuble", icon: purpleIcon, order: 2},
    {code: EtapeFtth.TERMINE_QUARTIER, title: "Quartier Terminé", icon: blueInvertedIcon, order: 3},
    {code: EtapeFtth.EN_COURS_QUARTIER, title: "Déploiement Quartier", icon: defaultIcon, order: 4},
    {code: EtapeFtth.PREVU_QUARTIER, title: "Quartier Programmé", icon: brownIcon, order: 5},
    {code: EtapeFtth.NON_PREVU, title: "Non prévu", icon: pinkIcon, order: 6},
    {code: EtapeFtth._, title: "Aucune donnée", icon: redIcon, order: 7},
    {code: EtapeFtth.UNKNOWN, title: "Statut inconnu", icon: blackIcon, order: 999}
];
const maxBounds = latLngBounds([
    [40.3097691, -6.0005669],
    [52.0544363, 11.9582151]
]);

const { mdAndDown } = useDisplay();
const { current } = useTheme();

const headers = [
    { title: 'Adresse', value: 'libAdresse' },
    { title: 'Création', value: 'created' },
    { title: 'Batiment', value: 'batiment', width: '250px' },
    { title: 'Éligibilité FTTH', value: 'eligibilitesFtth' },
    { title: 'Coord Lat', value: 'x' },
    { title: 'Coord Long', value: 'y' },
];

const toastStore = useToastStore();

const isDarkTheme = computed(() => {
    return current.value.dark;
})

//#endregion

// onMounted(() => {
//      centerMapOnLocation();
// });

function getCloseAreaFibers() {
    recentResult.value = resultFromDb.value = false;
    loading.value = true;
    layers.value = [];
    FiberService.getCloseAreaFibers(userLocation.value.x, userLocation.value.y)
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
    FiberService.updateWideArea(userLocation.value.x, userLocation.value.y)
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
    FiberService.getWideArea(userLocation.value.x, userLocation.value.y)
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
    FiberService.getDbFibers(userLocation.value.x, userLocation.value.y)
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
    FiberService.getNewestPoints(bounds.value!.toBBoxString())
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

function getHistorique(fiber: FiberPointDTO) {
    if (!resultFromDb.value) {
        loadingHistory.value = true;
        FiberService.getHistorique(fiber.signature)
        .then((response) => {
            fiber.created = response.data.created;
            fiber.lastUpdated = response.data.lastUpdated;
            fiber.eligibilitesFtth = response.data.eligibilitesFtth.sort((a, b) => (a.batiment < b.batiment && a.etapeFtth < b.etapeFtth ? -1 : 1));
            fiber.eligibilitesFtth.forEach(f => f.strEtapeFtth = EtapeFtth[f.etapeFtth]);
        })
        .catch((errors) => {
            toastStore.createToastMessage({ color:ISnackbarColor.Error, message: errors });
        })
        .finally(() => {
            loadingHistory.value = false;
        });
    }
}

function centerUpdate(center: LatLng) {
    userLocation.value = new Point(center.lat, center.lng);
}

function clearData() {
    fibers.value = [];
    layers.value = [];
}

function centerMapOnPoint(evt: PointerEvent, row: any) {
    userLocation.value = new Point(row.item.y, row.item.x);
    zoom.value = 19;
}

function setIcon(fiber: FiberPointDTO) {
    if (fiber.eligibilitesFtth.length > 0) {
        const icon = icons.filter(a => a.code === fiber.eligibilitesFtth[0].etapeFtth)[0]?.icon;
        if (icon === undefined) {
            fiber.iconUrl = blackIcon;
        }
        fiber.iconUrl = icon;
    } else {
        fiber.iconUrl = redIcon;
    }
    fiber.iconClassName = opacityWithElderness(fiber);
}

function showHideLayer(code: EtapeFtth) {
    const selectedLayer = layers.value.filter(l => l.name === code.toString())[0];
    selectedLayer.visible = !selectedLayer.visible;
}

function boundsUpdated (newBounds: L.LatLngBounds) {
    bounds.value = newBounds;
}

function mapFibersToLayer() {
    fibers.value.forEach(fiber => {
        setIcon(fiber);
        fiber.eligibilitesFtth.forEach(f => f.strEtapeFtth = EtapeFtth[f.etapeFtth]);
    });
    layers.value = [];
    Object.keys(EtapeFtth).forEach((value) => {
        layers.value.push(
            {
                markers: fibers.value.filter(f => f.eligibilitesFtth[0]?.etapeFtth === Number(value)),
                name: value.toString(),
                visible: true,
            }
        );
    });
    const noDataLayer = layers.value.filter(l => l.name === EtapeFtth._.toString())[0];
    noDataLayer.markers = noDataLayer.markers.concat(
        fibers.value.filter(f => f.eligibilitesFtth.length === 0)
    );
    const debugLayer = layers.value.filter(l => l.name === EtapeFtth.UNKNOWN.toString())[0];
    debugLayer.markers = debugLayer.markers.concat(
        fibers.value.filter(f => f.eligibilitesFtth?.some(x => x.etapeFtth === EtapeFtth.DEBUG))
    );
}

function centerMapOnLocation() {
    navigator.geolocation.getCurrentPosition(
        position => {
            userLocation.value = new Point(position.coords.latitude, position.coords.longitude);
        }, 
        error => toastStore.createToastMessage({ message: error.message, color: ISnackbarColor.Error }),
        {
            enableHighAccuracy: true,
            timeout: 10000
        });
}

function opacityWithElderness(fiber: FiberPointDTO) {
    if (!recentResult.value) {
        return 'opacity-100';
    }
    let mostRecentDate = Math.max(...fiber.eligibilitesFtth.map(f => new Date(f.lastUpdated).getTime()));
    if (fiber.eligibilitesFtth.length === 0) {
        mostRecentDate = new Date(fiber.lastUpdated).getTime();
    }
    let result = 100;
    if (mostRecentDate >= date.getTime() - 1 * 86400000) {
        result = 100;
    } else if (mostRecentDate >= date.getTime() - 3 * 86400000) {
        result = 60;
    } else if (mostRecentDate >= date.getTime() - 6 * 86400000) {
        result = 50;
    } else {
        result = 15;
    }
    return 'opacity-' + result;
}

const orderedIcons = computed(() => {
    return icons.sort((a, b) => a.order - b.order);
});

const mapHeight = computed(() => {
    return mdAndDown.value ? "50vh" : "75vh";
});
</script>