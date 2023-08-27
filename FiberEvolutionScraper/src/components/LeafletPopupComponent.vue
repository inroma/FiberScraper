<template>
    <v-container class="pa-0">
        <v-progress-circular v-if="loading" indeterminate color="primary" />
        <v-list v-else :key="'list-popup'+fiber?.signature" light max-height="400px" class="py-0 overflow-y-auto">
            <v-list-item v-if="fiber?.eligibilitesFtth?.length === 0" :key="'popup-list-'+fiber?.signature">
                <v-list-item-content>
                    <v-list-item-title>{{ fiber.libAdresse }}</v-list-item-title>
                    <v-list-item-subtitle>
                        <b>Created: </b> {{ new Date(fiber.created).toLocaleString('fr-FR')}}<br>
                        <b>Dernière MaJ: </b> {{ new Date(fiber.lastUpdated).toLocaleString('fr-FR')}}
                    </v-list-item-subtitle>
                </v-list-item-content>
            </v-list-item>
            <div v-for="item, i of fiber?.eligibilitesFtth" :key="'popup-list-'+item.codeImb+'-'+item.etapeFtth">
                <v-list-item class="px-0">
                    <v-list-item-content>
                        <v-list-item-title>{{ fiber?.libAdresse }}</v-list-item-title>
                        <v-list-item-subtitle v-if="!!item.batiment">
                            <b>Batiment: </b> {{ item.batiment }}<br>
                        </v-list-item-subtitle>
                        <v-list-item-subtitle>
                            <b>FTTH: </b> {{ item.strEtapeFtth }}<br>
                            <b>Created: </b> {{ new Date(item.created).toLocaleString()}}<br>
                            <b>Dernière MaJ: </b> {{ new Date(item.lastUpdated).toLocaleString()}}
                        </v-list-item-subtitle>
                    </v-list-item-content>
                </v-list-item>
                <v-divider v-if="i < fiber?.eligibilitesFtth.length-1"/>
            </div>
        </v-list>
    </v-container>
</template>
<script lang="ts" src="./LeafletPopupComponent.ts"/>