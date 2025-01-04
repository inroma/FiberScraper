<template>
    <VContainer>
        <VRow :dense="!smAndDown">
            <VCol>
                <VBtn @click="getNewestFibers()" color="primary" variant="flat" text="Charger nouveaux points" #append>
                    <VIcon>mdi-information-outline</VIcon>
                    <VTooltip activator="parent" location="bottom">
                        <span>Affiche les points ajoutés/modifiés en BDD depuis moins de 5 jours</span>
                    </VTooltip>
                </VBtn>
            </VCol>
            <VCol>
                <VBtn @click="updateFibers()" color="green-lighten-1" :loading="loading"
                text="Actualiser zone étendue" variant="tonal" #append>
                    <VIcon>mdi-information-outline</VIcon>
                    <VTooltip activator="parent" location="bottom">
                        <span>Requête une grande zone de données sur l'API Orange <br/>
                            pour les mettre à jour en BDD</span>
                    </VTooltip>
                </VBtn>
            </VCol>
            <VCol>
                <VBtn @click="getDbFibers()" variant="outlined" text="Charger fibres en BDD" #append>
                    <VIcon>mdi-information-outline</VIcon>
                    <VTooltip activator="parent" location="bottom">
                        <span>Affiche les raccordements fibres enregistrés en BDD</span>
                    </VTooltip>
                </VBtn>
            </VCol>
            <VCol>
                <VBtn @click="getFibers()" color="primary" :loading="loading" variant="tonal" text="Charger zone étendue" #append>
                    <VIcon>mdi-information-outline</VIcon>
                    <VTooltip activator="parent" location="bottom">
                        <span>Affiche les données de raccordement depuis <br/>l'API Orange sur une zone large</span>
                    </VTooltip>
                </VBtn>
            </VCol>
            <VCol>
                <VBtn @click="getCloseAreaFibers()" color="primary" :loading="loading" variant="tonal" text="Charger zone proche" #append>
                    <VIcon>mdi-information-outline</VIcon>
                    <VTooltip activator="parent" location="bottom">
                        <span>Affiche les données de raccordement depuis <br/>l'API Orange sur une zone restreinte</span>
                    </VTooltip>
                </VBtn>
            </VCol>
        </VRow>
    </VContainer>
</template>
<script setup lang="ts">
import { useDisplay } from 'vuetify';

const { smAndDown } = useDisplay();

const props = defineProps({
    loading: { type: Boolean }
});
const emit = defineEmits([ 'updateFibers', 'getDbFibers', 'getFibers', 'getCloseAreaFibers', 'getNewestFibers' ])

function updateFibers() {
    emit('updateFibers');
}

function getDbFibers() {
    emit('getDbFibers');
}

function getFibers() {
    emit('getFibers');
}

function getCloseAreaFibers() {
    emit('getCloseAreaFibers');
}

function getNewestFibers() {
    emit('getNewestFibers');
}
</script>