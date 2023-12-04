<style>
#mapContainer {
    z-index: 3;
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
                    <v-btn color="primary" @click="deleteItem(popupDeleteItem); deleteDialog = false">Confirmer</v-btn>
                    <v-btn color="secondary" @click="deleteDialog = false">Fermer</v-btn>
                </v-card-actions>
            </v-card>
        </v-dialog>
        <v-card-actions ref="menu">
        </v-card-actions>
        <div class="ml-10 mr-10">
            <v-row no-gutters>
                <v-responsive min-width="200">
                    <l-map id="mapContainer" :style="'height:'+mapHeight" ref="map" :center="userLocation" :zoom="zoom"
                    :max-bounds="maxBounds" @update:center="centerUpdate" @update:bounds="boundsUpdated" @click="controlOpened = false">
                        <!-- Background Tiles Controls -->
                        <l-control-layers key="control-layers" position="topright"/>
                        <!-- Background Tiles Providers -->
                        <l-tile-layer v-for="tileProvider of tileProviders"
                            :url="tileProvider.url" :attribution="tileProvider.attribution"
                            :key="tileProvider.name" :name="tileProvider.name" layer-type="base" :visible="tileProvider.visible"
                        />
                        <l-rectangle v-for="rectangle, i in rectangles" :key="'rectangle_'+i" :bounds="rectangle.getBounds()"
                            :color="rectangle.options.color" :fillColor="rectangle.options.fillColor"
                            :weight="rectangle.options.weight" :dashArray="rectangle.options.dashArray"/>
                        <l-rectangle v-for="rectangle, i in newRectangle" :key="'newRectangle_'+i" :bounds="rectangle.getBounds()"
                            :color="rectangle.options.color" :fillColor="rectangle.options.fillColor"
                            :weight="rectangle.options.weight" :dashArray="rectangle.options.dashArray"/>
                        <l-control key="control-custom" position='bottomleft' class="custom-control" disableScrollPropagation>
                            <v-btn icon @click="centerMapOnLocation()" color="primary" class="mx-2">
                                <v-icon>mdi-crosshairs-gps</v-icon>
                            </v-btn>
                        </l-control>
                        <l-control key="control-custom-legend" position='bottomright' class="custom-control px-2 ma-2" disableScrollPropagation>
                            <div class="mb-1"><v-icon color="red">mdi-square-outline</v-icon><span class="black--text mx-2">Taille d'un refresh hors ville</span></div>
                            <div><v-icon color="blue darken-3">mdi-square-outline</v-icon><span class="black--text mx-2">Taille d'un refresh exclusivement en ville</span></div>
                        </l-control>
                    </l-map>
                </v-responsive>
            </v-row>
            <v-row>
                <v-col>
                </v-col>
                <v-col>
                    <v-btn class="primary" @click="runAll">
                        <v-icon left dark>mdi-play-outline</v-icon>
                        Refresh manuel des zones
                    </v-btn>
                </v-col>
                <v-col>
                    <v-btn class="primary" @click="createItem" :disabled="autoRefreshItems.some(x => x.isEditing)">
                        <v-icon left dark>mdi-plus</v-icon>
                        Ajouter une zone
                    </v-btn>
                </v-col>
                <v-col>
                </v-col>
            </v-row>
            <v-row key="main-card-content" justify="center">
                <v-col md="11">
                    <v-data-table class="mt-5 mb-10" :headers="headers" key="list-details" :items="autoRefreshItems" fixed-header height="650px"
                    :loading="loading" @click:row="centerMapOnPoint" :sort-by="headers[0].value">
                        <template v-slot:[`item.id`]="{ item }">
                            <td>{{ item.id }}</td>
                        </template>
                        <template v-slot:item.enabled="{ item }">
                            <v-simple-checkbox :disabled="!item.isEditing" v-model="item.enabled" @click.stop/>
                        </template>
                        <template v-slot:item.label="{ item }">
                            <v-text-field :readonly="!item.isEditing" v-model="item.label" @click.stop solo flat hide-details/>
                        </template>
                        <template v-slot:item.coordX="{ item }">
                            <v-text-field :readonly="!item.isEditing" v-model="item.coordX" @click.stop solo flat hide-details/>
                        </template>
                        <template v-slot:item.coordY="{ item }">
                            <v-text-field :readonly="!item.isEditing" v-model="item.coordY" @click.stop solo flat hide-details/>
                        </template>
                        <template v-slot:item.lastRun="{ item }">
                            <td>
                                {{ item.lastRun !== undefined ? new Date(item.lastRun).toLocaleString() : 'Jamais exécuté' }}
                            </td>
                        </template>
                        <template v-slot:item.areaSize="{ item }">
                            <v-select :readonly="!item.isEditing || item.isEditing === false" :items="areaSizes" v-model="item.areaSize" @click.prevent
                            solo flat hide-details/>
                        </template>
                        <template v-slot:item.actions="{ item }">
                            <div v-if="item.isEditing" class="d-inline">
                                <v-tooltip v-if="item.id === 0" bottom>
                                    <template v-slot:activator="{ on }">
                                        <v-btn icon small @click.stop="addItem(item)" v-on="on">
                                            <v-icon color="primary">mdi-send-variant-outline</v-icon>
                                        </v-btn>
                                    </template>
                                    <span>Créer l'auto-refresh</span>
                                </v-tooltip>
                                <v-tooltip v-else bottom>
                                    <template v-slot:activator="{ on }">
                                        <v-btn icon small @click.stop="updateItem(item)" v-on="on">
                                            <v-icon color="green">mdi-content-save</v-icon>
                                        </v-btn>
                                    </template>
                                    <span>Enregistrer</span>
                                </v-tooltip>
                                <v-tooltip bottom>
                                    <template v-slot:activator="{ on }">
                                        <v-btn icon small @click.stop="getAutoRefreshInputs" v-on="on"><v-icon color="grey">mdi-cancel</v-icon></v-btn>
                                    </template>
                                    <span>Annuler</span>
                                </v-tooltip>
                            </div>
                            <div v-else class="d-inline">
                                <v-tooltip bottom>
                                    <template v-slot:activator="{ on }">
                                        <v-btn icon small @click.stop="editItem(item)" v-on="on"><v-icon>mdi-pencil</v-icon></v-btn>
                                    </template>
                                    <span>Éditer l'auto-refresh</span>
                                </v-tooltip>
                            </div>
                            <v-tooltip bottom>
                                <template v-slot:activator="{ on }">
                                    <v-btn icon small v-on="on" @click.stop="popupDeleteItem = item; deleteDialog = true"><v-icon color="red">mdi-delete-outline</v-icon></v-btn>
                                </template>
                                <span>Supprimer cet auto-refresh</span>
                            </v-tooltip>
                        </template>
                    </v-data-table>
                </v-col>
            </v-row>
        </div>
    </v-card>
</template>
<script lang="ts" src="./AutoRefreshView.ts"/>