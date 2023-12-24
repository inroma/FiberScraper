<style>
#mapContainer {
    z-index: 3;
    height: 70vh;
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
.v-data-table td {
  padding-left: 6px !important;
  padding-right: 6px !important;
}
</style>

<template>
    <v-card key="main-card" class="mx-0 mb-0">
        <v-dialog v-model="deleteDialog" width="auto" @click:outside="deleteDialog = false">
            <v-card>
                <v-card-title class="pa-3">
                    Confirmer la suppression
                </v-card-title>
                <v-card-text class="pa-5">
                    Voulez-vous vraiment supprimer cet item ?
                </v-card-text>
                <v-card-actions class="pb-4">
                    <v-spacer/>
                    <v-btn color="red" @click="deleteItem(popupDeleteItem!); deleteDialog = false">Confirmer</v-btn>
                    <v-btn color="grey" @click="deleteDialog = false">Fermer</v-btn>
                </v-card-actions>
            </v-card>
        </v-dialog>
        <v-card-actions ref="menu">
        </v-card-actions>
            <v-row class="ml-10 mr-10 h-full" no-gutters>
                <v-responsive min-width="200">
                    <l-map ref="map" :zoom="zoom" :minZoom="5" :center="[userLocation.lat, userLocation.lng]" :options="{ preferCanvas: true }"
                    :max-bounds="maxBounds" :style="{ height:mapHeight, width:'100%'}" @update:bounds="boundsUpdated" @update:center="centerUpdate">
                        <LControlLayers key="control-layers" position="topright"/>
                        <LTileLayer v-for="tileProvider of tileProviders"
                            :url="tileProvider.url" :attribution="tileProvider.attribution"
                            :key="tileProvider.name" :name="tileProvider.name" layer-type="base" :visible="tileProvider.visible"
                        />
                        <LRectangle v-for="rectangle, i in rectangles" :key="'rectangle_'+i" :bounds="rectangle.getBounds()"
                            :color="rectangle.options.color" :fillColor="rectangle.options.fillColor"
                            :weight="rectangle.options.weight" :dashArray="rectangle.options.dashArray"/>
                        <LRectangle v-for="rectangle, i in newRectangle" :key="'newRectangle_'+i" :bounds="rectangle.getBounds()"
                            :color="rectangle.options.color" :fillColor="rectangle.options.fillColor"
                            :weight="rectangle.options.weight" :dashArray="rectangle.options.dashArray"/>
                        <LControl key="control-custom" position='bottomleft' class="custom-control" disableScrollPropagation>
                            <v-icon size="28" @click="centerMapOnLocation()" color="primary" class="mx-2">mdi-crosshairs-gps</v-icon>
                        </LControl>
                        <LControl key="control-custom-legend" position='bottomright' class="custom-control px-2 ma-2" disableScrollPropagation>
                            <div class="mb-1"><v-icon color="red">mdi-square-outline</v-icon><span class="text-black mx-2">Taille d'un refresh hors ville</span></div>
                            <div><v-icon color="blue darken-3">mdi-square-outline</v-icon><span class="text-black mx-2">Taille d'un refresh exclusivement en ville</span></div>
                        </LControl>
                    </l-map>
                </v-responsive>
            </v-row>
            <v-row>
                <v-col>
                </v-col>
                <v-col>
                    <v-btn color="primary" @click="runAll">
                        <v-icon start>mdi-play-outline</v-icon>
                        Refresh manuel des zones
                    </v-btn>
                </v-col>
                <v-col>
                    <v-btn color="primary" @click="createItem()" :disabled="autoRefreshItems.some(x => x.isEditing)">
                        <v-icon start>mdi-plus</v-icon>
                        Ajouter une zone
                    </v-btn>
                </v-col>
                <v-col>
                </v-col>
            </v-row>
            <v-row key="main-card-content" justify="center">
                <v-col md="11">
                    <v-data-table class="mt-5 mb-10" :headers="headers" key="list-details" :items="autoRefreshItems" fixed-header height="650px"
                    items-per-page="25" :loading="loading" @click:row="centerMapOnPoint" :sort-by="[{ key: 'id', order: true }]">
                        <template #item.id="{ item }">
                            <td>{{ item.id }}</td>
                        </template>
                        <template #item.enabled="{ item }">
                            <v-checkbox :disabled="!item.isEditing" v-model="item.enabled" @click.stop hide-details/>
                        </template>
                        <template #item.label="{ item }">
                            <v-text-field :readonly="!item.isEditing" v-model="item.label" @click.stop solo flat hide-details/>
                        </template>
                        <template #item.coordX="{ item }">
                            <v-text-field :readonly="!item.isEditing" v-model="item.coordX" @click.stop solo flat hide-details/>
                        </template>
                        <template #item.coordY="{ item }">
                            <v-text-field :readonly="!item.isEditing" v-model="item.coordY" @click.stop solo flat hide-details/>
                        </template>
                        <template #item.lastRun="{ item }">
                            <td>
                                {{ item.lastRun !== undefined ? new Date(item.lastRun).toLocaleString() : 'Jamais exécuté' }}
                            </td>
                        </template>
                        <template #item.areaSize="{ item }">
                            <v-select :readonly="!item.isEditing" :items="areaSizes" v-model="item.areaSize" @click.stop
                            solo flat hide-details/>
                        </template>
                        <template #item.actions="{ item }">
                            <div v-if="item.isEditing" class="d-inline">
                                <v-btn class="mr-1" v-if="item.id === 0" icon density="compact" @click.stop="addItem(item)" flat>
                                    <v-icon color="primary">mdi-send-variant-outline</v-icon>
                                    <v-tooltip activator="parent" bottom>
                                        <span>Créer l'auto-refresh</span>
                                    </v-tooltip>
                                </v-btn>
                                <v-btn class="mr-1" icon density="compact" @click.stop="updateItem(item)" flat>
                                    <v-icon color="green">mdi-content-save</v-icon>
                                    <v-tooltip activator="parent" bottom>
                                        <span>Enregistrer</span>
                                    </v-tooltip>
                                </v-btn>
                                <v-btn class="mr-1" icon density="compact" @click.stop="getAutoRefreshInputs" flat>
                                    <v-icon color="grey">mdi-cancel</v-icon>
                                    <v-tooltip activator="parent" bottom>
                                        <span>Annuler</span>
                                    </v-tooltip>
                                </v-btn>
                            </div>
                            <div v-else class="d-inline">
                                <v-btn class="mr-1" icon density="compact" @click.stop="editItem(item)" flat>
                                    <v-icon>mdi-pencil</v-icon>
                                    <v-tooltip activator="parent" bottom>
                                        <span>Éditer l'auto-refresh</span>
                                    </v-tooltip>
                                </v-btn>
                            </div>
                                <v-btn icon density="compact" @click.stop="popupDeleteItem = item; deleteDialog = true" flat>
                                    <v-icon color="red">mdi-delete-outline</v-icon>
                                    <v-tooltip activator="parent" bottom>
                                        <span>Supprimer cet auto-refresh</span>
                                    </v-tooltip>
                                </v-btn>
                        </template>
                    </v-data-table>
                </v-col>
            </v-row>
    </v-card>
</template>
<script setup lang="ts">
import 'leaflet/dist/leaflet.css';
import L from "leaflet";
import { LMap, LControlLayers, LControl, LTileLayer, LRectangle } from '@vue-leaflet/vue-leaflet';
import { ISnackbarColor } from '@/models/SnackbarInterface';
import AutoRefreshInput from '@/models/AutoRefreshInput';
import AutoRefreshService from '@/services/AutoRefreshService';
import { useToastStore } from '@/store/ToastStore';
import { Ref, computed, onMounted, ref } from 'vue';
import { useDisplay } from 'vuetify'

//#region Public Properties
const loading = ref(false);
const userLocation = ref(new L.LatLng(45.76, 4.83));
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
    },
    {
        name: 'OpenTopoMap',
        visible: false,
        url: 'https://{s}.tile.opentopomap.org/{z}/{x}/{y}.png',
        attribution:
        'Map data: &copy; <a href="http://www.openstreetmap.org/copyright">OpenStreetMap</a>, <a href="http://viewfinderpanoramas.org">SRTM</a> | Map style: &copy; <a href="https://opentopomap.org">OpenTopoMap</a> (<a href="https://creativecommons.org/licenses/by-sa/3.0/">CC-BY-SA</a>)',
    }
];
const mapBounds = ref() as Ref<L.LatLngBounds>;
const maxBounds = L.latLngBounds([
    [40.3097691, -6.0005669],
    [52.0544363, 11.9582151]
]);
const autoRefreshItems = ref([]) as Ref<AutoRefreshInput[]>;
const rectangles = ref([]) as Ref<L.Rectangle[]>;
const newRectangle = ref([]) as Ref<L.Rectangle[]>;
const deleteDialog = ref(false);
const popupDeleteItem = ref(undefined) as Ref<AutoRefreshInput | undefined>;
const toastStore = useToastStore();
const { name } = useDisplay();

const headers = [
    { title: 'Id', value: 'id', align: 'center', width: '65px', key: "id" },
    { title: 'Label', value: 'label', align: 'center', width: '220px', key: "label" },
    { title: 'Coord Long', value: 'coordX', align: 'center', key: "coordX" },
    { title: 'Coord Lat', value: 'coordY', align: 'center', key: "coordY" },
    { title: 'Taille de la zone', value: 'areaSize', align: 'center', width: '150px', key: "areaSize" },
    { title: 'Dernier Refresh', value: 'lastRun', align: 'center', width: '170px', key: "lastRun" },
    { title: 'Activé', value: 'enabled', align: 'center', sortable: false, key: "enabled" },
    { title: 'Actions', value: 'actions', align: 'center', sortable: false, width: '110px', key: "actions" },
];

//#endregion

onMounted(() => {
    // centerMapOnLocation();
    getAutoRefreshInputs();
});

function getAutoRefreshInputs() {
    loading.value = true;
    AutoRefreshService.getAll()
    .then((response) => {
        autoRefreshItems.value = response.data;
        rectangles.value = autoRefreshItems.value.flatMap(item => {
            return getRectangleFromInput(item);
        });
        rectangles.value = rectangles.value.concat(autoRefreshItems.value.flatMap(item => {
            return getRectangleFromInput(item, true);
        }));
        newRectangle.value = [];
    })
    .catch((errors) => {
        toastStore.createToastMessage({ color: ISnackbarColor.Error, message: errors });
    })
    .finally(() => {
        loading.value = false;
    });
}

function createItem() {
    const item = new AutoRefreshInput();
    item.isEditing = true;
    item.coordX = userLocation.value.lng;
    item.coordY = userLocation.value.lat;
    autoRefreshItems.value.push(item);
    newRectangle.value = [getRectangleFromInput(item!), getRectangleFromInput(item!, true)];
    centerUpdate(userLocation.value);
}

function getRectangleFromInput(item: AutoRefreshInput, cityOffset: boolean = false) {
    const latOffset = cityOffset === true ? 0.004457 * .25 : 0.004457;
    const lngOffset = cityOffset === true ? 0.006929 * .25 : 0.006929;
    const bounds = new L.LatLng(item.coordY, item.coordX);
    const areaSizeDivided = item.areaSize/2;
    const rectangle = L.rectangle(
        [[bounds.lat - areaSizeDivided*latOffset, bounds.lng - areaSizeDivided*lngOffset],
            [bounds.lat + areaSizeDivided*latOffset, bounds.lng + areaSizeDivided*lngOffset]],
        {
            color: !item.enabled ? 'grey' : latOffset === 0.004457 ? 'red' : 'blue',
            fillColor: 'lightblue',
            weight: 2,
            dashArray: !item.enabled || item.id === 0 ? "6" : undefined
        });
    return rectangle;
}

function addItem(item: AutoRefreshInput) {
    loading.value = true;
    AutoRefreshService.add(item)
    .then((response) => {
        toastStore.createToastMessage({ color: ISnackbarColor.Success, message: `${response.data} zone créée avec succès` });
        getAutoRefreshInputs();
        newRectangle.value = [];
    })
    .catch((errors) => {
        toastStore.createToastMessage({ color: ISnackbarColor.Error, message: errors });
    })
    .finally(() => {
        loading.value = false;
    });
}

function updateItem(item: AutoRefreshInput) {
    loading.value = true;
    AutoRefreshService.update(item)
    .then((response) => {
        toastStore.createToastMessage({ color: ISnackbarColor.Success, message: `${response.data} zone mise à jour avec succès` });
    })
    .catch((errors) => {
        toastStore.createToastMessage({ color: ISnackbarColor.Error, message: errors });
    })
    .finally(() => {
        item.isEditing = false;
        loading.value = false;
    });
}

function deleteItem(item: AutoRefreshInput) {
    if (item.id === 0) {
        removeItem(item);
        if (item.isEditing) {
            newRectangle.value = [];
        }
    } else {
        loading.value = true;
        AutoRefreshService.delete(item.id)
        .then((response) => {
            toastStore.createToastMessage({ color: ISnackbarColor.Success, message: `${response.data} zone supprimée avec succès` });
            removeItem(item);
        })
        .catch((errors) => {
            toastStore.createToastMessage({ color: ISnackbarColor.Error, message: errors });
        })
        .finally(() => {
            popupDeleteItem.value = undefined;
            loading.value = false;
        });
    }
}

function removeItem(item: AutoRefreshInput) {
    if (item.id === 0) {
        newRectangle.value.map(m => m.remove());
    }
    const index = autoRefreshItems.value.findIndex(x => x === item);
    autoRefreshItems.value.splice(index, 1);
}

function boundsUpdated (bounds: L.LatLngBounds) {
    mapBounds.value = bounds;
}

function centerMapOnLocation() {
    navigator.geolocation.getCurrentPosition(
        position => {
            userLocation.value = new L.LatLng(position.coords.latitude, position.coords.longitude);
        }, 
        error => toastStore.createToastMessage({ message: error.message, color: ISnackbarColor.Error }),
        {
            enableHighAccuracy: true,
            timeout: 10000
        });
}

function centerUpdate(center: L.LatLng) {
    userLocation.value = center;
    if(autoRefreshItems.value.some(x => x.isEditing)) {
        const item = autoRefreshItems.value.find(x => x.isEditing);
        item!.coordX = center.lng;
        item!.coordY = center.lat;
        newRectangle.value.map((m, i)=> m.setLatLngs(getRectangleFromInput(item!, Boolean(i)).getLatLngs()));
    }
}

function centerMapOnPoint(evt: any, row: any) {
    userLocation.value = new L.LatLng(row.item.coordY, row.item.coordX);
    zoom.value = 16;
}

function editItem(point: AutoRefreshInput) {
    autoRefreshItems.value.map(x => x.isEditing = false);
    point.isEditing = true;
    centerUpdate(new L.LatLng(point.coordY, point.coordX));
}

function runAll() {
    AutoRefreshService.runAll()
    .then(() => {
        toastStore.createToastMessage({ message: "Refresh manuel des zones lancé", color: ISnackbarColor.Success });
    })
    .catch(() => {
        toastStore.createToastMessage({ message: "Erreur pendant le lancement manuel des refresh", color: ISnackbarColor.Error });
    })
}

const mapHeight = computed(() => {
    switch(name.value) {
        case 'xs':
        case 'md':
            return "50vh";
        default:
            return "75vh";
    }
});

/** AreaSizes disponibles pour alimenter le champ */
const areaSizes =  [1, 3, 5];

</script>