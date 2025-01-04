<script setup lang="ts">
import FiberPointDTO from '@/models/FiberPointDTO';
import { EtapeFtth } from '@/models/Enums';
import { useToastStore } from '@/store/ToastStore';
import { ISnackbarColor } from '@/models/SnackbarInterface';
import FiberService from '@/services/FiberService';

const fiber = defineModel<FiberPointDTO>('fiber', { default: null, required: false });
const props = defineProps<{
    resultFromDb: boolean
}>();
const loading = ref<boolean>(false);

const toastStore = useToastStore();

watch(fiber, () => getHistorique());

function getHistorique() {
    if (props.resultFromDb || !fiber.value) {
        return;
    }
    loading.value = true;
    FiberService.getHistorique(fiber.value.signature)
    .then((response) => {
        fiber.value.created = response.data.created;
        fiber.value.lastUpdated = response.data.lastUpdated;
        fiber.value.eligibilitesFtth = response.data.eligibilitesFtth
            .sort((a, b) => (a.batiment < b.batiment && a.etapeFtth < b.etapeFtth ? -1 : 1));
    })
    .catch((errors) => toastStore.createToastMessage({ color: ISnackbarColor.Error, message: errors }))
    .finally(() => loading.value = false);
}

</script>
<template>
    <VCard v-if="fiber" theme="light" min-width="280px" :key="fiber.signature">
        <VCardActions class="pa-0 justify-end" style="min-height: 0 !important;">
            <VBtn size="small" icon="mdi-window-close" @click="fiber = null"/>
        </VCardActions>
        <VCardText class="pa-0">
            <VProgressCircular v-if="loading" indeterminate color="primary" />
            <VList lines="two" :key="'list-popup'+fiber?.signature" max-height="400px" class="pa-0 overflow-y-auto" density="compact">
                <VListItem lines="two" v-if="fiber?.eligibilitesFtth?.length === 0" :key="'popup-list-'+fiber?.signature">
                    <VListItemTitle class="text-pre-wrap">{{ fiber.libAdresse }}</VListItemTitle>
                    <b>Création: </b>{{ new Date(fiber.created).toLocaleString('fr-FR')}}<br>
                    <b>Dernière MaJ: </b>{{ new Date(fiber.lastUpdated).toLocaleString('fr-FR')}}
                </VListItem>
                <div v-else>
                    <div v-for="item, i of fiber?.eligibilitesFtth" :key="'popup-list-'+item.codeImb+'-'+item.etapeFtth">
                        <VListItem lines="two">
                            <VListItemTitle class="text-pre-wrap">{{ fiber?.libAdresse }}</VListItemTitle>
                            <VListItemSubtitle v-if="!!item.batiment" class="pb-1 text-black">
                                <b>Batiment: </b>{{ item.batiment }}<br>
                            </VListItemSubtitle>
                            <b>FTTH: </b>{{ EtapeFtth[item.etapeFtth] }}<br>
                            <b>Création: </b>{{ new Date(item.created).toLocaleString()}}<br>
                            <b>Dernière MaJ: </b>{{ new Date(item.lastUpdated).toLocaleString()}}
                        </VListItem>
                        <VDivider v-if="i < fiber?.eligibilitesFtth.length-1" thickness="2"/>
                    </div>
                </div>
            </VList>
        </VCardText>
    </VCard>
</template>
