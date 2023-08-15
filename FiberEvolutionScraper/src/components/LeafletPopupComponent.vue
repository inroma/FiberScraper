<template>
    <v-container>
        <v-progress-circular v-if="loading" indeterminate color="primary" />
        <v-list v-else key="list-popup" light max-height="400px" class="overflow-y-auto">
            <v-list-item v-if="fiber?.eligibilitesFtth?.length === 0" :key="'popup-list-'+fiber?.signature">
                <v-list-item-content key="popup-list-content-no-data">
                    <v-list-item-title key="'popup-list-content-title-no-data'">{{ fiber.libAdresse }}</v-list-item-title>
                    <v-list-item-subtitle key="popup-list-content-subtitle-0">
                        <b>Created: </b> {{ new Date(fiber.created).toLocaleString('fr-FR')}}<br>
                        <b>Dernière MaJ: </b> {{ new Date(fiber.lastUpdated).toLocaleString('fr-FR')}}
                    </v-list-item-subtitle>
                </v-list-item-content>
            </v-list-item>
            <v-list-item v-for="item in fiber.eligibilitesFtth" :key="'popup-list-'+item.codeImb+'-'+item.etapeFtth">
                <v-list-item-content key="popup-list-content">
                    <v-list-item-title :key="'popup-list-content-title'+item.codeImb">{{ fiber.libAdresse }}</v-list-item-title>
                    <v-list-item-subtitle v-if="!!item.batiment" key="popup-list-content-subtitle-1">
                        <b>Batiment: </b> {{ item.batiment }}<br>
                    </v-list-item-subtitle>
                    <v-list-item-subtitle key="popup-list-content-subtitle-2">
                        <b>FTTH: </b> {{ item.strEtapeFtth }}<br>
                        <b>Created: </b> {{ new Date(item.created).toLocaleString()}}<br>
                        <b>Dernière MaJ: </b> {{ new Date(item.lastUpdated).toLocaleString()}}
                    </v-list-item-subtitle>
                </v-list-item-content>
            </v-list-item>
        </v-list>
    </v-container>
</template>
<script lang="ts" src="./LeafletPopupComponent.ts"/>