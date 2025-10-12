<script setup lang="ts">
import FiberPointDTO from '@/models/FiberPointDTO';
import { EtapeFtth } from '@/models/Enums';
import { useToastStore } from '@/store/ToastStore';
import { ISnackbarColor } from '@/models/SnackbarInterface';
import { fiberService } from '@/services/FiberService';
import type EligibiliteFtth from '@/models/EligibiliteFtthDTO';
import dayjs from 'dayjs';

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
    fiberService.getHistorique(fiber.value.signature)
    .then((response) => {
        fiber.value.created = response.data.created;
        fiber.value.lastUpdated = response.data.lastUpdated;
        fiber.value.eligibilitesFtth = response.data.eligibilitesFtth
            .sort((a, b) => (a.batiment < b.batiment && a.etapeFtth < b.etapeFtth ? -1 : 1));
    })
    .catch((errors) => toastStore.createToastMessage({ color: ISnackbarColor.Error, message: errors }))
    .finally(() => loading.value = false);
}

const groupedItems = computed(() => Object.groupBy<string, EligibiliteFtth>(fiber.value.eligibilitesFtth, (f) => f.codeImb));

</script>
<template>
    <VCard v-if="fiber" theme="light" min-width="300px" :key="fiber.signature">
        <div class="d-flex">
            <span class="pa-2">{{ fiber.libAdresse }}</span>
            <VSpacer />
            <VIcon class="align-self-center pe-4" size="small" icon="mdi-close" @click="fiber = null" />
        </div>
        <VCardText class="pa-0">
            <VProgressCircular v-if="loading" indeterminate color="primary" />
            <VExpansionPanels v-if="fiber.eligibilitesFtth?.length" tile variant="accordion" :model-value="fiber.eligibilitesFtth.at(0)?.codeImb">
                <VExpansionPanel v-for="bat, codeImb, i of groupedItems" :title="bat.at(i)?.batiment" :value="codeImb">
                    <VExpansionPanelText>
                        <VList lines="two" :key="'list-popup'+codeImb" max-height="400px" class="pa-0 overflow-y-auto" density="compact">
                            <div v-for="item, i of bat" :key="`popup-list-${codeImb}-${item.etapeFtth}-${i}`">
                                <VListItem lines="two">
                                    <b>FTTH: </b>{{ EtapeFtth[item.etapeFtth] }}<br>
                                    <b>Création: </b>{{ dayjs(item.created).format('DD/MM/YYYY') }}<br>
                                    <b>MaJ: </b>{{ dayjs(item.lastUpdated).format('DD/MM/YYYY') }}
                                </VListItem>
                                <VDivider v-if="i < bat.length-1" thickness="2"/>
                            </div>
                        </VList>
                    </VExpansionPanelText>
                </VExpansionPanel>
            </VExpansionPanels>
            <div v-else>
                <VListItem lines="two" :key="'popup-list-'+fiber?.signature">
                    <b>Création: </b>{{ dayjs(fiber.created).format('DD/MM/YYYY') }}<br>
                    <b>MaJ: </b>{{ dayjs(fiber.lastUpdated).format('DD/MM/YYYY') }}
                </VListItem>
            </div>
        </VCardText>
    </VCard>
</template>
