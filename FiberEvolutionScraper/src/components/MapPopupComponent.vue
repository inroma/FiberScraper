<template>
    <VContainer class="pa-0">
        <VProgressCircular v-if="loading" indeterminate color="primary" />
        <VList v-else :key="'list-popup'+fiber?.signature" theme="light" max-height="310px" class="pa-0 overflow-y-auto" density="compact">
            <VListItem v-if="fiber?.eligibilitesFtth?.length === 0" :key="'popup-list-'+fiber?.signature" class="px-2">
                <VListItemTitle>{{ fiber.libAdresse }}</VListItemTitle>
                <b>Création: </b>{{ new Date(fiber.created).toLocaleString('fr-FR')}}<br>
                <b>Dernière MaJ: </b>{{ new Date(fiber.lastUpdated).toLocaleString('fr-FR')}}
            </VListItem>
            <div v-else>
                <div v-for="item, i of fiber?.eligibilitesFtth" :key="'popup-list-'+item.codeImb+'-'+item.etapeFtth">
                    <VListItem class="px-2">
                        <VListItemTitle>{{ fiber?.libAdresse }}</VListItemTitle>
                        <VListItemSubtitle v-if="!!item.batiment" class="pb-1 text-black">
                            <b>Batiment: </b>{{ item.batiment }}<br>
                        </VListItemSubtitle>
                        <b>FTTH: </b>{{ item.strEtapeFtth }}<br>
                        <b>Création: </b>{{ new Date(item.created).toLocaleString()}}<br>
                        <b>Dernière MaJ: </b>{{ new Date(item.lastUpdated).toLocaleString()}}
                    </VListItem>
                    <VDivider v-if="i < fiber?.eligibilitesFtth.length-1"/>
                </div>
            </div>
        </VList>
    </VContainer>
</template>
<script setup lang="ts">
import FiberPointDTO from '@/models/FiberPointDTO';
import { PropType } from 'vue';

const props = defineProps({
    fiber: { type: Object as PropType<FiberPointDTO>, required: true, default: null },
    loading: { type: Boolean, required: true, default: false }
})

</script>