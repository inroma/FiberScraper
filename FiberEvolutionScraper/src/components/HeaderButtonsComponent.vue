<template>
    <v-container>
        <v-row dense>
            <v-col>
                <v-btn @click="updateFibers()" color="green lighten-1" :loading="loading">Actualiser zone étendue</v-btn>
            </v-col>
            <v-col>
                <v-btn @click="getDbFibers()">Charger fibres en BDD</v-btn>
            </v-col>
            <v-col>
                <v-btn @click="getFibers()" color="primary" :loading="loading">Charger zone étendue</v-btn>
            </v-col>
            <v-col>
                <v-btn @click="getCloseAreaFibers()" color="primary" :loading="loading">Charger zone proche</v-btn>
            </v-col>
            <v-col>
                <v-btn @click="getNewestFibers()" color="blue accent-4">Charger nouveaux points</v-btn>
            </v-col>
        </v-row>
    </v-container>
</template>
<script lang="ts" src="./HeaderButtonsComponent.ts"/>