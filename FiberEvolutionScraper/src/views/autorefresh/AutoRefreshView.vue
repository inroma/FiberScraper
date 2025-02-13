<script setup lang="ts">
import { ISnackbarColor } from '@/models/SnackbarInterface';
import AutoRefreshInput from '@/models/AutoRefreshInput';
import AutoRefreshService from '@/services/AutoRefreshService';
import { useToastStore } from '@/store/ToastStore';
import { useDisplay } from 'vuetify';
import { Map, Layers, Sources, Styles, Geometries } from 'vue3-openlayers';
import { storeToRefs } from 'pinia';
import { useMapStore } from '@/store/mapStore';

//#region Public Properties
const loading = ref(false);
const autoRefreshItems = ref<AutoRefreshInput[]>([]);
const deleteDialog = ref(false);
const popupDeleteItem = ref<AutoRefreshInput>(undefined);
const toastStore = useToastStore();
const { view, location } = storeToRefs(useMapStore());
const { mdAndDown, mobile } = useDisplay();

const rectangles = computed<number[][][][]>(() => autoRefreshItems.value?.filter(a => a.enabled && !a.isEditing)?.flatMap(item => getRectangleFromInput(item)));
const smallRectangles = computed<number[][][][]>(() => autoRefreshItems.value?.filter(a => a.enabled && !a.isEditing)?.flatMap(item => getRectangleFromInput(item, true)));
const disabledRectangles = computed(() => {
    return autoRefreshItems.value?.filter(a => !a.enabled)?.map(item => 
        getRectangleFromInput(item).at(0).concat(getRectangleFromInput(item, true).at(0))
    );
});
const newRectangle = computed<number[][][][]>(() => {
    let value: number[][][][] = [];
    autoRefreshItems.value?.filter(a => a.isEditing || a.id === 0)?.forEach(item => {
        value = getRectangleFromInput(item).concat(getRectangleFromInput(item, true));
    });
    return value;
});
const lineDash = [7, 7];

const headers = [
    { title: 'Id', value: 'id', sortable: true, width: '65px', key: "id" },
    { title: 'Label', value: 'label', sortable: true, width: '220px', key: "label" },
    { title: 'Coord Long', value: 'coordX', sortable: true, key: "coordX" },
    { title: 'Coord Lat', value: 'coordY', sortable: true, key: "coordY" },
    { title: 'Taille de la zone', value: 'areaSize', sortable: true, width: '150px', key: "areaSize" },
    { title: 'Dernier Refresh', value: 'lastRun', sortable: true, width: '170px', key: "lastRun" },
    { title: 'Activé', value: 'enabled', sortable: true, key: "enabled" },
    { title: 'Actions', value: 'actions', sortable: false, width: '110px', key: "actions" },
];

//#endregion

onMounted(() => {
    getAutoRefreshInputs();
});

function getAutoRefreshInputs() {
    loading.value = true;
    AutoRefreshService.getAll()
    .then((response) => autoRefreshItems.value = response.data)
    .catch((errors) => toastStore.createToastMessage({ color: ISnackbarColor.Error, message: errors }))
    .finally(() => loading.value = false);
}

function createItem() {
    const item = new AutoRefreshInput();
    item.isEditing = true;
    item.coordX = location.value.at(0);
    item.coordY = location.value.at(1);
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
    .finally(() => loading.value = false);
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
    if(autoRefreshItems.value.some(x => x.isEditing)) {
        const item = autoRefreshItems.value.find(x => x.isEditing);
        item!.coordX = location.value[0];
        item!.coordY = location.value[1];
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

/** AreaSizes disponibles pour alimenter le champ */
const areaSizes =  [1, 3, 5];

</script>
<template>
    <VCard key="main-card" :class="mobile ? 'mx-0' : 'mx-10'">
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
        <VRow justify="center">
            <VCol lg="4">
                <AddressLookup />
            </VCol>
        </VRow>
        <MapComponent class="py-5" :loading="loading" force-move-end-event @callback="centerUpdate">
            <template #ui>
                <div class="overlay right">
                    <div class="custom-control text-caption px-2 ma-2 mb-6">
                        <div class="mb-1"><VIcon color="red">mdi-square-outline</VIcon><span class="text-black mx-2">Taille d'un refresh hors ville</span></div>
                        <div><VIcon color="blue-darken-3">mdi-square-outline</VIcon><span class="text-black mx-2">Taille d'un refresh exclusivement en ville</span></div>
                    </div>
                </div>
            </template>
            <template #vectorLayers>
                <Layers.OlVectorLayer title="Zones" base-layer>
                    <Sources.OlSourceVector>
                        <Map.OlFeature>
                            <Geometries.OlGeomMultiPolygon :coordinates="rectangles" />
                            <Styles.OlStyle>
                                <Styles.OlStyleStroke color="red" :width="2" />
                                <Styles.OlStyleFill color="rgba(186,236,255,0.18)" />
                            </Styles.OlStyle>
                        </Map.OlFeature>
                        <Map.OlFeature>
                            <Geometries.OlGeomMultiPolygon :coordinates="smallRectangles" />
                            <Styles.OlStyle>
                                <Styles.OlStyleStroke color="blue" :width="2" />
                                <Styles.OlStyleFill color="rgba(186,236,255,0.18)" />
                            </Styles.OlStyle>
                        </Map.OlFeature>
                        <Map.OlFeature>
                            <Geometries.OlGeomMultiPolygon :coordinates="disabledRectangles" />
                            <Styles.OlStyle>
                                <Styles.OlStyleStroke color="grey" :width="2" :lineDash="lineDash" />
                            </Styles.OlStyle>
                        </Map.OlFeature>
                        <Map.OlFeature v-if="newRectangle?.length > 0">
                            <Geometries.OlGeomMultiPolygon :coordinates="[newRectangle.at(0)]"/>
                            <Styles.OlStyle>
                                <Styles.OlStyleStroke color="red" :width="2" :lineDash="lineDash" />
                                <Styles.OlStyleFill color="rgba(186,236,255,0.18)" />
                            </Styles.OlStyle>
                        </Map.OlFeature>
                        <Map.OlFeature v-if="newRectangle?.length > 0">
                            <Geometries.OlGeomMultiPolygon :coordinates="[newRectangle.at(1)]"/>
                            <Styles.OlStyle>
                                <Styles.OlStyleStroke color="blue" :width="2" :lineDash="lineDash" />
                                <Styles.OlStyleFill color="rgba(186,236,255,0.18)" />
                            </Styles.OlStyle>
                        </Map.OlFeature>
                    </Sources.OlSourceVector>
                </Layers.OlVectorLayer>
            </template>
        </MapComponent>
        <VRow align="center">
            <VCol>
                <VBtn color="primary" @click="runAll()" :icon="mdAndDown">
                    <template #prepend>
                        Refresh manuel des zones
                    </template>
                    <template #default>
                        <VIcon class="ml-1">mdi-play-outline</VIcon>
                    </template>
                </VBtn>
            </VCol>
            <VCol>
                <VBtn color="primary" @click="createItem()" :disabled="autoRefreshItems.some((x: any) => x.isEditing)" :icon="mdAndDown">
                    <template #prepend>
                        Ajouter une zone
                    </template>
                    <template #default>
                        <VIcon>mdi-plus</VIcon>
                    </template>
                </VBtn>
            </VCol>
            <VCol>
                <VBtn icon @click="getAutoRefreshInputs()" variant="outlined">
                    <VIcon>mdi-reload</VIcon>
                </VBtn>
            </VCol>
        </VRow>
        <VRow key="main-card-content" justify="center">
            <VCol md="11">
                <VDataTable class="mt-5 mb-10" :headers="headers" :header-props="{ align: 'center' }" key="list-details"
                :items="autoRefreshItems" fixed-header height="650px" :mobile='null' mobile-breakpoint="md"
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
                            {{ item.lastRun !== null ? new Date(item.lastRun).toLocaleString() : 'Jamais exécuté' }}
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
<style src="@/assets/css/main.css" scoped/>
