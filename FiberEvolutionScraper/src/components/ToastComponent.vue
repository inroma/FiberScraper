<style scoped>    
    ::v-deep .v-snack__wrapper {
        max-width: 600px;
        overflow-wrap: break-word;
        line-break: strict;
    }
    .snackbar-div {
        display: flex;
        position: absolute;
        flex-direction: column;
        z-index: 100;
        overflow-y: hidden;
        width: 95%;
        height: 100%;
        pointer-events: none
    }
    .snackbar-item {
        height: auto;
        justify-content: flex-end;
        height: auto !important;
        position: relative !important;
    }
</style>
<template>
    <div class="snackbar-div">
        <v-snackbar v-for="snackbar in snackbars" :key="snackbar.id" :index="snackbar.id" transition="slide-y-transition"
            class="snackbar-item" v-model="snackbar.show" :color="snackbar.color" :timeout="snackbar.timeout">
            <v-icon class="pr-3">{{ icon(snackbar) }}</v-icon>
            <template v-slot:action>
                <v-btn class="pl-5" small @click="removeToast(snackbar)" icon :absolute="snackbar.message.length > 300 ? true : false" top right>
                    <v-icon class="pr-3">mdi-window-close</v-icon>
                </v-btn>
            </template>
            {{ snackbar.message }}
        </v-snackbar>
    </div>
</template>
<script lang="ts" src="./ToastComponent.ts"/>