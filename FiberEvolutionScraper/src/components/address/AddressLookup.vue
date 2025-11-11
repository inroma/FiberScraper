<script setup lang="ts">
import type { Feature } from '@/models/address/addressResponse';
import { ISnackbarColor } from '@/models/SnackbarInterface';
import { addressService } from '@/services/AddressService';
import { useMapStore } from '@/store/mapStore';
import { useToastStore } from '@/store/ToastStore';
import { storeToRefs } from 'pinia';

var addresses = ref<Feature[]>([]);
var search = ref<string>("");
var loading = ref<boolean>(false);
const mapStore = useMapStore();
let timeout = 650;
let selectedItem = ref<Feature>();

const { view } = storeToRefs(useMapStore());
const toastStore = useToastStore();

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

function centerMapOnLocation() {
	navigator.geolocation.getCurrentPosition(
		position => {
			view.value.setZoom(15);
			view.value.setCenter([position.coords.longitude, position.coords.latitude]);
		},
		error => toastStore.createToastMessage({ message: error.message, color: ISnackbarColor.Error }),
		{
			enableHighAccuracy: true,
			timeout: 4000
		});
}

</script>
<template>
	<VContainer class="d-inline-flex">
		<VAutocomplete class="pr-15" label="Adresse" hide-details :items="addresses"
				v-model:search="search" v-model:model-value="selectedItem"
				:loading="loading" density="comfortable" clearable
				@update:search="onInput()" @update:model-value="onSelectedAddress"
				item-title="properties.label" return-object>
			<template #prepend>
				<VBtn icon @click="centerMapOnLocation()" color="transparent" flat>
					<VIcon color="primary">mdi-crosshairs-gps</VIcon>
				</VBtn>
			</template>
		</VAutocomplete>
	</VContainer>
</template>