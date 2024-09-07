<script setup lang="ts">
import { ISnackbarColor } from '@/models/SnackbarInterface';
import AutoRefreshInput from '@/models/AutoRefreshInput';
import AutoRefreshService from '@/services/AutoRefreshService';
import { useToastStore } from '@/store/ToastStore';
import { Ref, computed, onMounted, ref } from 'vue';
import { useDisplay } from 'vuetify'
import { Map, View } from 'ol';

//#region Public Properties
const loading = ref(false);
const userLocation = ref<[number, number]>([4.83, 45.76]);
const zoom = ref(11);
const tileLayers = [
    {
        name: 'OpenStreetMap',
        visible: true,
        attribution:
        '&copy; <a target="_blank" href="http://osm.org/copyright">OpenStreetMap</a> contributors',
        url: 'https://tile.openstreetmap.org/{z}/{x}/{y}.png'
    },
    {
        name: 'CartoDBDark',
        visible: false,
        url: 'https://basemaps.cartocdn.com/dark_all/{z}/{x}/{y}{r}.png',
        attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors &copy; <a href="https://carto.com/attributions">CARTO</a>',
    },
    {
        name: 'OpenTopoMap',
        visible: false,
        url: 'https://tile.opentopomap.org/{z}/{x}/{y}.png',
        attribution:
        'Map data: &copy; <a href="http://www.openstreetmap.org/copyright">OpenStreetMap</a>, <a href="http://viewfinderpanoramas.org">SRTM</a> | Map style: &copy; <a href="https://opentopomap.org">OpenTopoMap</a> (<a href="https://creativecommons.org/licenses/by-sa/3.0/">CC-BY-SA</a>)',
    }
];
const maxBounds = [ -6, 41, 9.5, 52 ];
const autoRefreshItems = ref([]) as Ref<AutoRefreshInput[]>;
const deleteDialog = ref(false);
const popupDeleteItem = ref(undefined) as Ref<AutoRefreshInput | undefined>;
const toastStore = useToastStore();
const { name } = useDisplay();

const rectangles = computed<Number[][][][]>(() => autoRefreshItems.value?.filter(a => a.enabled && !a.isEditing)?.flatMap(item => getRectangleFromInput(item)));
const smallRectangles = computed<Number[][][][]>(() => autoRefreshItems.value?.filter(a => a.enabled && !a.isEditing)?.flatMap(item => getRectangleFromInput(item, true)));
const disabledRectangles = computed(() => {
    let value: Number[][][][] = [];
    autoRefreshItems.value?.filter(a => !a.enabled)?.forEach(item => {
        value = getRectangleFromInput(item).concat(getRectangleFromInput(item, true));
    });
    return value;
});
const newRectangle = computed<Number[][][][]>(() => {
    let value: Number[][][][] = [];
    autoRefreshItems.value?.filter(a => a.isEditing || a.id === 0)?.forEach(item => {
        value = getRectangleFromInput(item).concat(getRectangleFromInput(item, true));
    });
    return value;
});
const lineDash = [7, 7];

const map = ref<Map>();
const view = ref<View>();

const headers = [
    { title: 'Id', value: 'id', sortable: true, width: '65px', key: "id" },
    { title: 'Label', value: 'label', sortable: true, width: '220px', key: "label" },
    { title: 'Coord Long', value: 'coordX', sortable: true, key: "coordX" },
    { title: 'Coord Lat', value: 'coordY', sortable: true, key: "coordY" },
    { title: 'Taille de la zone', value: 'areaSize', sortable: true, width: '150px', key: "areaSize" },
    { title: 'Dernier Refresh', value: 'lastRun', sortable: true, width: '170px', key: "lastRun" },
    { title: 'Activé', value: 'enabled', sortable: false, key: "enabled" },
    { title: 'Actions', value: 'actions', sortable: false, width: '110px', key: "actions" },
];

//#endregion

onMounted(() => {
    getAutoRefreshInputs();
});

function getAutoRefreshInputs() {
    loading.value = true;
    AutoRefreshService.getAll()
    .then((response) => {
        autoRefreshItems.value = response.data;
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
    item.coordX = userLocation.value.at(0)!;
    item.coordY = userLocation.value.at(1)!;
    autoRefreshItems.value.push(item);
    centerUpdate();
}

function getRectangleFromInput(item: AutoRefreshInput, cityOffset: boolean = false) {
    const latOffset = cityOffset ? 0.004457 * .25 : 0.004457;
    const lngOffset = cityOffset ? 0.006929 * .25 : 0.006929;
    const coords = { lat: item.coordY, lng: item.coordX };
    const areaSizeDivided = item.areaSize/2;
    const topLeft = [coords.lng - areaSizeDivided*lngOffset, coords.lat - areaSizeDivided*latOffset];
    const topRight = [coords.lng + areaSizeDivided*lngOffset, coords.lat - areaSizeDivided*latOffset];
    const bottomLeft = [coords.lng - areaSizeDivided*lngOffset, coords.lat + areaSizeDivided*latOffset];
    const bottomRight = [coords.lng + areaSizeDivided*lngOffset, coords.lat + areaSizeDivided*latOffset];
    const rectangle = [[[topLeft, topRight, bottomRight, bottomLeft, topLeft]]];
    return rectangle;
}

function addItem(item: AutoRefreshInput) {
    loading.value = true;
    AutoRefreshService.add(item)
    .then((response) => {
        toastStore.createToastMessage({ color: ISnackbarColor.Success, message: `${response.data} zone créée avec succès` });
        getAutoRefreshInputs();
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
            autoRefreshItems.value = autoRefreshItems.value.filter(a => !a.isEditing);
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
    const index = autoRefreshItems.value.findIndex(x => x === item);
    autoRefreshItems.value.splice(index, 1);
}

function centerMapOnLocation() {
    navigator.geolocation.getCurrentPosition(
        position => {
            view.value?.setZoom(14);
            view.value?.setCenter([position.coords.longitude, position.coords.latitude]);
        }, 
        error => toastStore.createToastMessage({ message: error.message, color: ISnackbarColor.Error }),
        {
            enableHighAccuracy: true,
            timeout: 10000
        });
}

function centerUpdate() {
    const center = view.value?.getCenter();
    if(autoRefreshItems.value.some(x => x.isEditing)) {
        const item = autoRefreshItems.value.find(x => x.isEditing);
        item!.coordX = center![0];
        item!.coordY = center![1];
    }
}

function centerMapOnPoint(_: any, row: any) {
    view.value?.setZoom(14);
    view.value?.setCenter([row.item.coordX, row.item.coordY]);
}

function editItem(point: AutoRefreshInput) {
    autoRefreshItems.value.map(x => x.isEditing = false);
    point.isEditing = true;
    view.value?.setZoom(14);
    view.value?.setCenter([point.coordX, point.coordY]);
    centerUpdate();
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
<template>
    <VCard key="main-card" class="mx-0 mb-0">
        <VDialog v-model="deleteDialog" width="auto" @click:outside="deleteDialog = false">
            <VCard>
                <VCardTitle class="pa-3">
                    Confirmer la suppression
                </VCardTitle>
                <VCardText class="pa-5">
                    Voulez-vous vraiment supprimer cet item ?
                </VCardText>
                <VCardActions class="pb-4">
                    <VSpacer/>
                    <VBtn color="red" @click="deleteItem(popupDeleteItem!); deleteDialog = false">Confirmer</VBtn>
                    <VBtn color="grey" @click="deleteDialog = false">Fermer</VBtn>
                </VCardActions>
            </VCard>
        </VDialog>
        <VCardActions ref="menu">
        </VCardActions>
            <VRow class="ml-10 mr-10 h-full" no-gutters>
                <VResponsive min-width="200">
                    <OlMap ref="map" :style="{ height:mapHeight, width:'100%' }" loadTilesWhileAnimating loadTilesWhileInteracting
                    @moveend="centerUpdate">
                        <OlView ref="view" :center="userLocation" :zoom="zoom" :rotation="0" :extent="maxBounds" smoothExtentConstraint/>
                        <OlTileLayer v-for="tile, i in tileLayers" :title="tile.name" :visible="tile.visible" :key="'tileLayer_'+i">
                            <OlSourceOsm :url="tile.url" :attribution="tile.attribution" />
                        </OlTileLayer>
                        <OlLayerswitcherimageControl mouseover :selection="true"/>
                        <OlVectorLayer title="Zones">
                            <ol-source-vector>
                                <ol-feature>
                                    <ol-geom-multi-polygon :coordinates="rectangles" />
                                    <ol-style>
                                        <ol-style-stroke color="red" :width="2" />
                                        <ol-style-fill color="rgba(186,236,255,0.18)" />
                                    </ol-style>
                                </ol-feature>
                                <ol-feature>
                                    <ol-geom-multi-polygon :coordinates="smallRectangles" />
                                    <ol-style>
                                        <ol-style-stroke color="blue" :width="2" />
                                        <ol-style-fill color="rgba(186,236,255,0.18)" />
                                    </ol-style>
                                </ol-feature>
                                <ol-feature>
                                    <ol-geom-multi-polygon :coordinates="disabledRectangles" />
                                    <ol-style>
                                        <ol-style-stroke color="grey" :width="2" :lineDash="lineDash" />
                                        <ol-style-fill color="rgba(0,0,0,0)" />
                                    </ol-style>
                                </ol-feature>
                                <ol-feature v-if="newRectangle?.length > 0">
                                    <ol-geom-multi-polygon :coordinates="[newRectangle.at(0)]" />
                                    <ol-style>
                                        <ol-style-stroke color="red" :width="2" :lineDash="lineDash" />
                                        <ol-style-fill color="rgba(186,236,255,0.18)" />
                                    </ol-style>
                                </ol-feature>
                                <ol-feature v-if="newRectangle?.length > 0">
                                    <ol-geom-multi-polygon :coordinates="[newRectangle.at(1)]" />
                                    <ol-style>
                                        <ol-style-stroke color="blue" :width="2" :lineDash="lineDash" />
                                        <ol-style-fill color="rgba(186,236,255,0.18)" />
                                    </ol-style>
                                </ol-feature>
                            </ol-source-vector>
                        </OlVectorLayer>
                        <div class="overlay right">
                            <div class="custom-control text-caption px-2 ma-2 mb-6">
                                <div class="mb-1"><VIcon color="red">mdi-square-outline</VIcon><span class="text-black mx-2">Taille d'un refresh hors ville</span></div>
                                <div><VIcon color="blue darken-3">mdi-square-outline</VIcon><span class="text-black mx-2">Taille d'un refresh exclusivement en ville</span></div>
                            </div>
                        </div>
                        <!-- <div class="overlay left">
                            <div class="custom-control ma-2">
                                <VIcon size="28" @click="centerMapOnLocation()" color="primary" class="mx-2" icon="mdi-crosshairs-gps" />
                            </div>
                        </div> -->
                    </OlMap>
                </VResponsive>
            </VRow>
            <VRow>
                <VCol>
                </VCol>
                <VCol>
                    <VBtn color="primary" @click="runAll()" text="Refresh manuel des zones" #prepend>
                        <VIcon>mdi-play-outline</VIcon>
                    </VBtn>
                </VCol>
                <VCol>
                    <VBtn color="primary" @click="createItem()" :disabled="autoRefreshItems.some(x => x.isEditing)" text="Ajouter une zone" #prepend>
                        <VIcon>mdi-plus</VIcon>
                    </VBtn>
                </VCol>
                <VCol>
                    <VBtn class="float-end mr-10" icon @click="getAutoRefreshInputs()" variant="outlined">
                        <VIcon>mdi-reload</VIcon>
                    </VBtn>
                </VCol>
            </VRow>
            <VRow key="main-card-content" justify="center">
                <VCol md="11">
                    <VDataTable class="mt-5 mb-10" :headers="headers" :header-props="{ align: 'center' }" key="list-details" :items="autoRefreshItems" fixed-header height="650px"
                    items-per-page="25" :loading="loading" @click:row="centerMapOnPoint" :sort-by="[{ key: 'id', order: true }]">
                        <template #item.enabled="{ item }">
                            <VCheckbox :disabled="!item.isEditing" v-model="item.enabled" @click.stop hide-details/>
                        </template>
                        <template #item.label="{ item }">
                            <VTextField :readonly="!item.isEditing" v-model="item.label" @click.stop solo flat hide-details/>
                        </template>
                        <template #item.coordX="{ item }">
                            <VTextField :readonly="!item.isEditing" v-model="item.coordX" @click.stop solo flat hide-details/>
                        </template>
                        <template #item.coordY="{ item }">
                            <VTextField :readonly="!item.isEditing" v-model="item.coordY" @click.stop solo flat hide-details/>
                        </template>
                        <template #item.lastRun="{ item }">
                            <td>
                                {{ item.lastRun !== undefined ? new Date(item.lastRun).toLocaleString() : 'Jamais exécuté' }}
                            </td>
                        </template>
                        <template #item.areaSize="{ item }">
                            <VSelect :readonly="!item.isEditing" :items="areaSizes" v-model="item.areaSize" @click.stop
                            solo flat hide-details/>
                        </template>
                        <template #item.actions="{ item }">
                            <div v-if="item.isEditing" class="d-inline">
                                <VBtn class="mr-1" v-if="item.id === 0" icon density="compact" @click.stop="addItem(item)" flat>
                                    <VIcon color="primary">mdi-send-variant-outline</VIcon>
                                    <VTooltip activator="parent" bottom>
                                        <span>Créer l'auto-refresh</span>
                                    </VTooltip>
                                </VBtn>
                                <VBtn class="mr-1" icon density="compact" @click.stop="updateItem(item)" flat>
                                    <VIcon color="green">mdi-content-save</VIcon>
                                    <VTooltip activator="parent" bottom>
                                        <span>Enregistrer</span>
                                    </VTooltip>
                                </VBtn>
                                <VBtn class="mr-1" icon density="compact" @click.stop="getAutoRefreshInputs" flat>
                                    <VIcon color="grey">mdi-cancel</VIcon>
                                    <VTooltip activator="parent" bottom>
                                        <span>Annuler</span>
                                    </VTooltip>
                                </VBtn>
                            </div>
                            <div v-else class="d-inline">
                                <VBtn class="mr-1" icon density="compact" @click.stop="editItem(item)" flat>
                                    <VIcon>mdi-pencil</VIcon>
                                    <VTooltip activator="parent" bottom>
                                        <span>Éditer l'auto-refresh</span>
                                    </VTooltip>
                                </VBtn>
                            </div>
                                <VBtn icon density="compact" @click.stop="popupDeleteItem = item; deleteDialog = true" flat>
                                    <VIcon color="red">mdi-delete-outline</VIcon>
                                    <VTooltip activator="parent" bottom>
                                        <span>Supprimer cet auto-refresh</span>
                                    </VTooltip>
                                </VBtn>
                        </template>
                    </VDataTable>
                </VCol>
            </VRow>
    </VCard>
</template>
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
.overlay {
    position: absolute;
    display: block;
    bottom: 0;
    z-index: 1;
}
.right {
    right: 0;
}
.left {
    left: 0;
}
.v-data-table td {
  padding-left: 6px !important;
  padding-right: 6px !important;
}
</style>
