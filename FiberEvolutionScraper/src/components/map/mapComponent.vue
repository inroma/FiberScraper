<script setup lang="ts">
import { useMapStore } from '@/store/mapStore';
import { Map, Layers, Sources, MapControls } from 'vue3-openlayers';
import { storeToRefs } from 'pinia';

const popupPosition = defineModel<number[]>("popupPosition");
const mapLoading = defineModel<boolean>('loading');

const props = defineProps<{
    showPopup?: boolean,
    forceMoveEndEvent?: boolean
}>();

const emit = defineEmits<{
    callback: []
}>();

const { view, map } = storeToRefs(useMapStore());
const mapStore = useMapStore();
let lastEmitedCoordinates: number[];
let treshold = 0.2;
let lastZoom: number;
const zoomDiffThreshold = 1;

const layers = ref([
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
]);

onMounted(() => {
    view.value.setZoom(mapStore.zoom);
    view.value.setCenter(mapStore.location);
})

function moveEnd() {
    if (view.value) {
        mapStore.location = view.value.getCenter();
        mapStore.zoom = view.value.getZoom();
    }
    if (props.forceMoveEndEvent || coordinatesNeedNewCall()) {
        lastEmitedCoordinates = mapStore.location;
        lastZoom = mapStore.zoom;
        emit('callback');
    }
}

function coordinatesNeedNewCall() {
    const newCoords = view.value.getCenter();
    if (!newCoords) {
        return false;
    }
    // treshold = Dernière taille de map connue pendant une requête (70% de la taille pour trigger avant)
    let temp = view.value?.calculateExtent().map(a => a * 0.7);
    treshold = Math.min(Math.abs(temp[0] - temp[2]), Math.abs(temp[1] - temp[3]));
    if (!lastEmitedCoordinates || !lastZoom) {
        return true;
    }

    const moveCondition = Math.abs(lastEmitedCoordinates?.at(0) - newCoords.at(0)) > treshold ||
        Math.abs(lastEmitedCoordinates?.at(1) - newCoords.at(1)) > treshold;
    const zoomCondition = Math.abs(lastZoom - view.value?.getZoom()) > zoomDiffThreshold;

    return moveCondition || zoomCondition;
};

</script>
<template>
    <div align="center">
        <VResponsive :aspect-ratio="16/9" max-width="85%" max-height="75vh" min-height="480px">
            <Map.OlMap ref="map" style="height: 100%;" loadTilesWhileAnimating loadTilesWhileInteracting @moveend="moveEnd()">
                <Map.OlView ref="view" :extent="mapStore.maxBounds" :max-zoom="20" smoothExtentConstraint style="min-width: 400px; min-height: 400px;"/>
                <Layers.OlTileLayer v-for="tile, i in layers" :title="tile.name" :visible="tile.visible" :key="'tileLayer_'+i">
                    <Sources.OlSourceOsm :url="tile.url" :attributions="tile.attribution"/>
                </Layers.OlTileLayer>
                <MapControls.OlLayerswitcherimageControl mouseover />
                <slot name="vectorLayers">
                    <Layers.OlVectorLayer v-if="$slots.features">
                        <Sources.OlSourceVector>
                            <slot name="features"></slot>
                        </Sources.OlSourceVector>
                    </Layers.OlVectorLayer>
                </slot>
                <Map.OlOverlay key="map-popup" v-if="showPopup" :position="popupPosition" :offset="[0, -42]">
                    <VOverlay class="align-center d-flex flex-column-reverse" persistent model-value contained no-click-animation>
                        <VExpandTransition>
                            <slot name="popup"></slot>
                        </VExpandTransition>
                    </VOverlay>
                </Map.OlOverlay>
                <VOverlay model-value contained persistent :scrim="false" no-click-animation height="100%" width="100%" z-index="1" style="position: relative;">
                    <slot name="overlay">
                        <VProgressLinear v-if="mapLoading" indeterminate color="primary"/>
                    </slot>
                </VOverlay>
            </Map.OlMap>
            <slot name="ui"></slot>
        </VResponsive>
    </div>
</template>