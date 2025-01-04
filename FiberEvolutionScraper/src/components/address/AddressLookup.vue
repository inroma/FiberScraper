<script setup lang="ts">
import type { Feature } from '@/models/address/addressResponse';
import { addressService } from '@/services/AddressService';
import { useMapStore } from '@/store/mapStore';

var addresses = ref<Feature[]>([]);
var search = ref<string>("");
var loading = ref<boolean>(false);
const mapStore = useMapStore();
let timeout = 650;
let selectedItem = ref<Feature>();

function onInput() {
    clearTimeout(timeout);
    timeout = setTimeout(() => {
        queryApi();
    }, 650);
}

function queryApi() {
    // handle click on item
    if (search.value == selectedItem.value?.properties?.label) {
        return;
    }
    loading.value = true;
    addressService.search(search.value)
    .then((res) => {
        addresses.value = res.features;
    })
    .finally(() => loading.value = false);
}

function onSelectedAddress(item: Feature) {
    if (!item) {
        return;
    }
    mapStore.view?.setZoom(17);
    mapStore.view?.setCenter(item.geometry.coordinates);
}

</script>
<template>
    <VContainer>
        <VAutocomplete label="Adresse" hide-details :items="addresses" v-model:search="search" v-model:model-value="selectedItem"
                       :loading="loading" density="comfortable" clearable
                       @update:search="onInput()" @update:model-value="onSelectedAddress"
                       item-title="properties.label" return-object/>
    </VContainer>
</template>