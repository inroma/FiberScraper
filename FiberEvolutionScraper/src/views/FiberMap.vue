<style>
.red-icon {
    filter: hue-rotate(147deg) !important;
}
.green-icon {
    filter: hue-rotate(257deg) !important;
}
.purple-icon {
    filter: hue-rotate(50deg) !important;
}
.brown-icon {
    filter: hue-rotate(199deg) !important;
}
.black-icon {
    filter: brightness(0) !important;
}
.blue-inverted-icon {
    filter: invert() hue-rotate(185deg) !important;
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
}
#mapContainer {
    z-index: 3;
    height: 100%;
    width: 100%;
}
.custom-layer-dark {
    filter: saturate(0.7);
}
</style>

<template>
    <v-card>
        <v-row ref="menu">
            <v-col class="ml-3 mb-3" :cols="2" :lg="10" :md="10" :offset-md="2" :sm="9">
                <v-btn class="mr-3" @click="getFibers()" color="primary" :loading="loading">Charger zone étendue</v-btn>
                <v-btn class="mr-3" @click="getDbFibers()" color="primary" :loading="loading">Charger fibres en BDD</v-btn>
                <v-btn @click="getCloseAreaFibers()" color="primary" :loading="loading">Charger zone proche</v-btn>
            </v-col>
            <v-col class="mb-3 mr-3 text-right">
                <v-btn @click="clearData()" color="error" :loading="loading" :disabled="!fibers.length">Clear</v-btn>
            </v-col>
        </v-row>
        <div class="mb-10">
                <l-map id="mapContainer" ref="map" :center="userLocation" :zoom="zoom" @update:center="centerUpdate" style="height:900px;">
                    <l-control-layers position="topright"/>
                    <l-tile-layer v-for="tileProvider in tileProviders"
                        :url="tileProvider.url" :attribution="tileProvider.attribution"
                        :key="tileProvider.name" :name="tileProvider.name" layer-type="base" :visible="tileProvider.visible"/>
                    <l-layer-group :class="{ 'custom-layer-dark': $vuetify.theme.dark }"></l-layer-group>
                    <l-marker v-for="fiber, i in fibers" visible :ref="'marker_'+fiber.signature" :key="'marker_'+i" :icon="getIcon(fiber)"
                        :lat-lng="[fiber.y, fiber.x]">
                        <l-popup :content="fiber.libAdresse + '<br>FTTH: ' + fiber.etapeFtth"/>
                    </l-marker>
                    <l-control position='bottomleft' :class="{ 'custom-control': !$vuetify.theme.dark, 'custom-control-dark': $vuetify.theme.dark }">
                        <v-btn style="z-index:2;" class="ma-2" x-small text v-for="icon, i in icons" :key="i" @click="layerSelected">
                            <v-img contain height="27" width="20" :src="icon.icon.options.iconUrl" :class="icon.icon.options.className"/>
                            <span class="pl-2">{{ icon.title }}</span>
                        </v-btn>
                    </l-control>
                </l-map>
            <v-row justify="center">
                <v-col md="8">
                    <v-data-table class="mt-5 mb-10" :headers="headers" :items="fibers" fixed-header height="650px"
                        :options="{ itemsPerPage: 30 }" :loading="loading" @click:row="centerMapOnPoint">
                    </v-data-table>
                </v-col>
            </v-row>
        </div>
    </v-card>
</template>
<script lang="ts" src="./FiberMap.ts"/>