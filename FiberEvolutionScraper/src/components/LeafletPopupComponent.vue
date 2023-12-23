<template>
    <v-container class="pa-0">
        <v-progress-circular v-if="loading" indeterminate color="primary" />
        <v-list v-else :key="'list-popup'+fiber?.signature" theme="light" max-height="310px" class="pa-0 overflow-y-auto" density="compact">
            <v-list-item v-if="fiber?.eligibilitesFtth?.length === 0" :key="'popup-list-'+fiber?.signature" class="px-2">
                <v-list-item-title>{{ fiber.libAdresse }}</v-list-item-title>
                <b>Création: </b>{{ new Date(fiber.created).toLocaleString('fr-FR')}}<br>
                <b>Dernière MaJ: </b>{{ new Date(fiber.lastUpdated).toLocaleString('fr-FR')}}
            </v-list-item>
            <div v-else>
                <div v-for="item, i of fiber?.eligibilitesFtth" :key="'popup-list-'+item.codeImb+'-'+item.etapeFtth">
                    <v-list-item class="px-2">
                        <v-list-item-title>{{ fiber?.libAdresse }}</v-list-item-title>
                        <v-list-item-subtitle v-if="!!item.batiment" class="pb-1 text-black">
                            <b>Batiment: </b>{{ item.batiment }}<br>
                        </v-list-item-subtitle>
                        <b>FTTH: </b>{{ item.strEtapeFtth }}<br>
                        <b>Création: </b>{{ new Date(item.created).toLocaleString()}}<br>
                        <b>Dernière MaJ: </b>{{ new Date(item.lastUpdated).toLocaleString()}}
                    </v-list-item>
                    <v-divider v-if="i < fiber?.eligibilitesFtth.length-1"/>
                </div>
            </div>
        </v-list>
    </v-container>
</template>
<script setup lang="ts">
import FiberPointDTO from '@/models/FiberPointDTO';
import { PropType } from 'vue';

const props = defineProps({
    fiber: { type: Object as PropType<FiberPointDTO>, required: true, default: null },
    loading: { type: Boolean, required: true, default: false }
})

</script>