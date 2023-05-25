<style>
.leaflet-popup-content {
    min-width: 350px;
}
.leaflet-dark-tile {
    filter: hue-rotate(180deg) invert(100%) !important;
}
.custom-control {
  background: rgba(255, 255, 255, 0.785);
  padding: 0.5em;
  padding-bottom: 1em;
  padding-top: 1em;
  margin: 0.3em;
  border: 1px solid #aaa;
  border-radius: 0.7em;
  flex-direction: column !important;
  display: flex;
  align-items: flex-start;
  pointer-events: auto;
}
.custom-control-dark {
  background: rgba(90, 90, 90, 0.785);
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
    height:900px;
}
</style>

<template>
    <v-card key="main-card">
        <v-row ref="menu" class="mb-3">
            <v-col class="ml-3 text-left">
                <v-btn icon @click="centerMapOnLocation()" color="primary">
                    <v-icon>mdi-crosshairs-gps</v-icon>
                </v-btn>
            </v-col>
            <v-col class="ml-3 justify-center" :cols="2" :lg="10" :md="10" :offset-md="2" :sm="9">
                <v-btn class="mr-5" @click="updateFibers()" color="green lighten-1" :loading="loading">Actualiser zone étendue</v-btn>
                <v-btn class="mr-5" @click="getDbFibers()">Charger fibres en BDD</v-btn>
                <v-btn class="mr-5" @click="getFibers()" color="primary" :loading="loading">Charger zone étendue</v-btn>
                <v-btn class="mr-5" @click="getCloseAreaFibers()" color="primary" :loading="loading">Charger zone proche</v-btn>
                <v-btn @click="getNewestFibers()" color="blue accent-4">Charger nouveaux points</v-btn>
            </v-col>
            <v-col class="mr-3 text-right">
                <v-btn @click="clearData()" color="error" :disabled="!fibers.length">Clear</v-btn>
            </v-col>
        </v-row>
        <div class="mb-10 ml-10 mr-10">
            <l-map id="mapContainer" ref="map" :center="userLocation" :zoom="zoom"
            @update:center="centerUpdate" @update:bounds="boundsUpdated">
                <l-control-layers key="control-layers" position="topright"/>
                <l-tile-layer v-for="tileProvider in tileProviders"
                    :url="tileProvider.url" :attribution="tileProvider.attribution"
                    :key="tileProvider.name" :name="tileProvider.name" layer-type="base" :visible="tileProvider.visible"
                />
                <l-layer-group v-for="layer in layers" :visible="layer.visible" :key="'layer_'+layer.name">
                    <l-marker
                        v-for="fiber in layer.markers"
                        :ref="'marker_'+fiber.signature" :key="'marker_'+layer.name+'marker_'+fiber.signature"
                        :lat-lng="[fiber.y, fiber.x]" @click="getHistorique(fiber)">
                        <l-icon :icon-url="fiber?.iconUrl" class="leaflet-marker-icon" :class-name="recentResult ? fiber?.iconClassName : 'opacity-100'"/>
                        <l-popup :visible="false" key="popup">
                            <v-progress-circular v-if="loadingHistory"
                                indeterminate key="popup-loader"
                                color="primary"
                            />
                            <v-list v-else key="list-popup" light max-height="400px" class="overflow-y-auto">
                                <v-list-item v-if="fiber.eligibilitesFtth.length === 0" :key="'popup-list-'+fiber.signature">
                                    <v-list-item-content key="popup-list-content-no-data">
                                        <v-list-item-title key="'popup-list-content-title-no-data'">{{ fiber.libAdresse }}</v-list-item-title>
                                        <v-list-item-subtitle key="popup-list-content-subtitle-0">
                                            <b>Created: </b> {{ new Date(fiber.created).toLocaleString('fr-FR')}}<br>
                                            <b>Dernière MaJ: </b> {{ new Date(fiber.lastUpdated).toLocaleString('fr-FR')}}
                                        </v-list-item-subtitle>
                                    </v-list-item-content>
                                </v-list-item>
                                <v-list-item v-else v-for="item in fiber.eligibilitesFtth" :key="'popup-list-'+item.codeImb+'-'+item.etapeFtth">
                                    <v-list-item-content key="popup-list-content">
                                        <v-list-item-title :key="'popup-list-content-title'+item.codeImb">{{ fiber.libAdresse }}</v-list-item-title>
                                        <v-list-item-subtitle v-if="!!item.batiment" key="popup-list-content-subtitle-1">
                                            <b>Batiment: </b> {{ item.batiment }}<br>
                                        </v-list-item-subtitle>
                                        <v-list-item-subtitle key="popup-list-content-subtitle-2">
                                            <b>FTTH: </b> {{ item.strEtapeFtth }}<br>
                                            <b>Created: </b> {{ new Date(item.created).toLocaleString()}}<br>
                                            <b>Dernière MaJ: </b> {{ new Date(item.lastUpdated).toLocaleString()}}
                                        </v-list-item-subtitle>
                                    </v-list-item-content>
                                </v-list-item>
                            </v-list>
                        </l-popup>
                    </l-marker>
                </l-layer-group>
                <l-control key="control-custom" position='bottomleft' :class="{ 'custom-control': !$vuetify.theme.dark, 'custom-control-dark': $vuetify.theme.dark }">
                    <v-btn style="z-index:2;" class="ma-2" x-small text
                    v-for="icon in orderedIcons"
                    :key="'control-custom-btn-'+icon.code" @click="showHideLayer(icon.code)" :ripple="false"
                    :plain="!layers[icon.code]?.visible"
                    :disabled="!layers[icon.code] || layers[icon.code]?.markers?.length === 0">
                        <v-img :key="'control-custom-btn-img-'+icon.code" contain height="27" width="20" :src="icon.icon"/>
                        <span :key="'control-custom-btn-span-'+icon.code" class="pl-2">{{ icon.title }}</span>
                    </v-btn>
                </l-control>
            </l-map>
            <v-row key="main-card-content" justify="center">
                <v-col md="10">
                    <v-data-table class="mt-5 mb-10" :headers="headers" key="list-details" :items="fibers" fixed-header height="650px"
                        :options="{ itemsPerPage: 100 }" :loading="loading" @click:row="centerMapOnPoint"
                        :footer-props="{ 'items-per-page-options': [50, 100, 200, 500, 1000] }"
                    >
                        <template v-slot:[`item.eligibilitesFtth`]="{ item }">
                            <td key="list-item-eligibilite">
                                {{ item.eligibilitesFtth[0]?.strEtapeFtth }}
                            </td>
                        </template>
                        <template v-slot:[`item.batiment`]="{ item }">
                            <td key="list-item-batiment">
                                {{ item.eligibilitesFtth[0]?.batiment }}
                            </td>
                        </template>
                        <template v-slot:[`item.created`]="{ item }">
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
<script lang="ts" src="./FiberMap.ts"/>