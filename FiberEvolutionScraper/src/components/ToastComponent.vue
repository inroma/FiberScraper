<style scoped>    
    ::v-deep .v-snack__wrapper {
        max-width: 600px;
        overflow-wrap: break-word;
        line-break: strict;
    }
    ::v-deep .v-snack__content {
        margin-right: 20px;
    }
    .snackbar-div {
        display: flex;
        position: fixed;
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
        <v-snackbar v-for="snackbar in snackbars" :key="snackbar.id" :index="snackbar.id" transition="slide-x-reverse-transition"
            class="snackbar-item" v-model="snackbar.show" :color="snackbar.color" :timeout="snackbar.timeout"
            @mouseleave.native="snackbar.mouseOver = false" @mouseenter.native="snackbar.showtime = snackbar.timeout; snackbar.mouseOver = true">
            <v-icon v-if="getIcon(snackbar)" class="pr-2">
                {{ getIcon(snackbar) }}
            </v-icon>

            {{ snackbar.message }}

            <template v-slot:action>
                <v-btn small @click="hideToast(snackbar)" icon :absolute="snackbar.message.length > 300 ? true : false" top right>
                    <v-icon>mdi-window-close</v-icon>
                </v-btn>
            </template>
            <v-progress-linear absolute color="white" bottom :value="Math.floor(100 * (snackbar.showtime / snackbar.timeout))" />
        </v-snackbar>
    </div>
</template>
<script lang="ts" src="./ToastComponent.ts"/>