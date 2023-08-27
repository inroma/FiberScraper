<template>
    <v-container>
        <v-row dense>
            <v-col>
                <v-tooltip bottom>
                    <template v-slot:activator="{ on }">
                        <v-btn @click="updateFibers()" color="green lighten-1" :loading="loading">
                            Actualiser zone étendue
                            <v-icon small color="grey lighten-2" v-on="on" right>mdi-information-outline</v-icon>
                        </v-btn>
                    </template>
                    <span>Requête une grande zone de données sur l'API Orange <br/>
                        pour les mettre à jour en BDD</span>
                </v-tooltip>
            </v-col>
            <v-col>
                <v-tooltip bottom>
                    <template v-slot:activator="{ on }">
                        <v-btn @click="getDbFibers()">Charger fibres en BDD
                            <v-icon small color="grey lighten-2" v-on="on" right>mdi-information-outline</v-icon>
                        </v-btn>
                    </template>
                    <span>Affiche les raccordements fibres enregistrés en BDD</span>
                </v-tooltip>
            </v-col>
            <v-col>
                <v-tooltip bottom>
                    <template v-slot:activator="{ on }">
                        <v-btn @click="getFibers()" color="primary" :loading="loading">Charger zone étendue
                            <v-icon small color="grey lighten-2" v-on="on" right>mdi-information-outline</v-icon>
                        </v-btn>
                    </template>
                    <span>Affiche les données de raccordement depuis <br/> l'API Orange sur une zone large</span>
                </v-tooltip>
            </v-col>
            <v-col>
                <v-tooltip bottom>
                    <template v-slot:activator="{ on }">
                        <v-btn @click="getCloseAreaFibers()" color="primary" :loading="loading">Charger zone proche
                            <v-icon small color="grey lighten-2" v-on="on" right>mdi-information-outline</v-icon>
                        </v-btn>
                    </template>
                    <span>Affiche les données de raccordement depuis <br/> l'API Orange sur une zone restreinte</span>
                </v-tooltip>
            </v-col>
            <v-col>
                <v-tooltip bottom>
                    <template v-slot:activator="{ on }">
                        <v-btn @click="getNewestFibers()" color="blue accent-4">Charger nouveaux points
                            <v-icon small color="grey lighten-2" v-on="on" right>mdi-information-outline</v-icon>
                        </v-btn>
                    </template>
                    <span>Affiche les points ajoutés/modifiés depuis moins de 5 jours</span>
                </v-tooltip>
            </v-col>
        </v-row>
    </v-container>
</template>
<script lang="ts" src="./HeaderButtonsComponent.ts"/>