<style>
.leaflet-popup-content {
    min-width: 350px;
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
  background: rgba(90, 90, 90, 0.785);
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
            <header-banner-buttons class="d-sm-none-and-down" :loading="loading" @updateFibers="updateFibers" @getDbFibers="getDbFibers"
                @getFibers="getFibers" @getCloseAreaFibers="getCloseAreaFibers" @getNewestFibers="getNewestFibers"/>
            <v-btn class="mr-5" @click="clearData()" color="error" :disabled="!fibers.length">Clear</v-btn>
        </v-card-actions>
        <div class="ml-10 mr-10">
            <v-row no-gutters>
                <v-responsive min-height="350" min-width="100">
                    <l-map id="mapContainer" :style="'height:'+mapHeight" ref="map" :center="userLocation" :zoom="zoom"
                    :max-bounds="maxBounds" @update:center="centerUpdate" @update:bounds="boundsUpdated" @click="controlOpened = false">
                        <!-- Background Tiles Controls -->
                        <l-control-layers key="control-layers" position="topright"/>
                        <!-- Background Tiles Providers -->
                        <l-tile-layer v-for="tileProvider of tileProviders"
                            :url="tileProvider.url" :attribution="tileProvider.attribution"
                            :key="tileProvider.name" :name="tileProvider.name" layer-type="base" :visible="tileProvider.visible"
                        />
                        <!-- Map Layers -->
                        <l-layer-group v-for="layer of layers" :visible="layer.visible" :key="'layer_'+layer.name">
                            <l-marker v-for="fiber of layer.markers" :ref="'marker_'+fiber.signature" :key="'layer_'+layer.name+'marker_'+fiber.signature"
                            :lat-lng="[fiber.y, fiber.x]" @click="getHistorique(fiber)">
                                <l-icon :icon-url="fiber.iconUrl" class="leaflet-marker-icon" :class-name="fiber?.iconClassName"/>
                                <l-popup :key="'popup'+fiber.signature+fiber.x">
                                    <leaflet-popup-content :fiber="fiber" :loading="loadingHistory"/>
                                </l-popup>
                            </l-marker>
                        </l-layer-group>
                        <!-- Layers visibility controls -->
                        <l-control v-if="$vuetify.breakpoint.mdAndDown && !controlOpened" key="control-custom" position='bottomleft' :class="['custom-control', { 'custom-control-dark': $vuetify.theme.dark }]" disableScrollPropagation>
                            <v-btn small text :ripple="false" @click.stop="controlOpened = true">
                                <v-icon>mdi-map-marker-multiple-outline</v-icon>
                            </v-btn>
                        </l-control>
                        <l-control v-else key="control-custom" position='bottomleft' :class="['custom-control', { 'custom-control-dark': $vuetify.theme.dark }]" disableScrollPropagation>
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
                </v-responsive>
            </v-row>
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