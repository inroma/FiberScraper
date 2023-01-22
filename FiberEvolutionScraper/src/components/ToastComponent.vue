<style scoped>    
    ::v-deep .v-snack__wrapper {
        max-width: 600px;
        overflow-wrap: break-word;
        line-break: strict;
    }
</style>
<template>
    <div name="snackbars">
        <v-snackbar class="mt-5 mr-5" v-model="snackbar.show" :color="snackbar.color" :timeout="snackbar.timeout" top right>
            <v-icon class="pr-3">{{ icon }}</v-icon>
            <template v-slot:action>
                <v-btn small @click="snackbar.show = false" text :absolute="snackbar.message.length > 500 ? true : false" top right>Fermer</v-btn>
            </template>
            {{ snackbar.message }}
        </v-snackbar>
    </div>
</template>
<script lang="ts" src="./ToastComponent.ts"/>