<template>
    <v-container>
        <v-row :dense="!smAndDown">
            <v-col>
                <v-btn @click="getNewestFibers()" color="primary" variant="flat" text="Charger nouveaux points" #append>
                    <v-icon small>mdi-information-outline</v-icon>
                    <v-tooltip activator="parent" location="bottom">
                        <span>Affiche les points ajoutés/modifiés en BDD depuis moins de 5 jours</span>
                    </v-tooltip>
                </v-btn>
            </v-col>
            <v-col>
                <v-btn @click="updateFibers()" color="green lighten-1" :loading="loading"
                text="Actualiser zone étendue" variant="tonal" #append>
                    <v-icon small end>mdi-information-outline</v-icon>
                    <v-tooltip activator="parent" location="bottom">
                        <span>Requête une grande zone de données sur l'API Orange <br/>
                            pour les mettre à jour en BDD</span>
                    </v-tooltip>
                </v-btn>
            </v-col>
            <v-col>
                <v-btn @click="getDbFibers()" variant="outlined" text="Charger fibres en BDD" #append>
                    <v-icon small end>mdi-information-outline</v-icon>
                    <v-tooltip activator="parent" location="bottom">
                        <span>Affiche les raccordements fibres enregistrés en BDD</span>
                    </v-tooltip>
                </v-btn>
            </v-col>
            <v-col>
                <v-btn @click="getFibers()" color="primary" :loading="loading" variant="tonal" text="Charger zone étendue" #append>
                    <v-icon small end>mdi-information-outline</v-icon>
                    <v-tooltip activator="parent" location="bottom">
                        <span>Affiche les données de raccordement depuis <br/> l'API Orange sur une zone large</span>
                    </v-tooltip>
                </v-btn>
            </v-col>
            <v-col>
                <v-btn @click="getCloseAreaFibers()" color="primary" :loading="loading" variant="tonal" text="Charger zone proche" #append>
                    <v-icon small end>mdi-information-outline</v-icon>
                    <v-tooltip activator="parent" location="bottom">
                        <span>Affiche les données de raccordement depuis <br/> l'API Orange sur une zone restreinte</span>
                    </v-tooltip>
                </v-btn>
            </v-col>
        </v-row>
    </v-container>
</template>
<script setup lang="ts">
import { useDisplay } from 'vuetify/lib/framework.mjs';


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