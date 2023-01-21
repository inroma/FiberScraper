<template>
    <div name="snackbars">
        <v-snackbar class="mt-5 mr-5" v-model="snackbar.show" :color="snackbar.color" :timeout="snackbar.timeout" top right>
        <v-icon class="pr-3">{{ icon }}</v-icon>
        {{ snackbar.message }}
        <template v-slot:action="{ attrs }">
            <v-btn small text v-bind="attrs" @click="snackbar.show = false">Fermer</v-btn>
        </template>
        </v-snackbar>
    </div>
</template>
<script lang="ts" src="./ToastComponent.ts"/>