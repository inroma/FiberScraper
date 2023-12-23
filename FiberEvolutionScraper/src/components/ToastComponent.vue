<style scoped>    
    :deep(.v-snack__wrapper) {
        max-width: 600px;
        overflow-wrap: break-word;
        line-break: strict;
    }
    :deep(.v-snack__content) {
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
        <v-snackbar v-for="snackbar in toastStore.getToasts" :key="snackbar.id" :index="snackbar.id" transition="slide-x-reverse-transition"
            class="snackbar-item" v-model="snackbar.show" :color="snackbar.color" :timeout="snackbar.timeout" timer="white"
            @mouseleave.native="snackbar.mouseOver = false" @mouseenter.native="snackbar.showtime = snackbar.timeout; snackbar.mouseOver = true">
            <v-icon v-if="getIcon(snackbar)" class="pr-2">
                {{ getIcon(snackbar) }}
            </v-icon>

            {{ snackbar.message }}

            <template #actions>
                <v-btn small @click="hideToast(snackbar)" icon :absolute="snackbar.message.length > 300 ? true : false" top right>
                    <v-icon>mdi-window-close</v-icon>
                </v-btn>
            </template>
        </v-snackbar>
    </div>
</template>
<script setup lang="ts">
import { ISnackbarColor, Snackbar } from '@/models/SnackbarInterface';
import { useToastStore } from '@/store/ToastStore';


const toastStore = useToastStore();

function hideToast(snack: Snackbar) {
    snack.show = false;
}

function getIcon(snack: Snackbar) {
    let icon = "";
    if (snack.icon !== undefined) {
        icon = snack.icon;
    } else {
        switch (snack.color) {
            case ISnackbarColor.Success: 
                icon = "mdi-check";
                break;
            case ISnackbarColor.Info: 
                icon = "mdi-information-outline";
                break;
            case ISnackbarColor.Warning: 
                icon = "mdi-alert-circle-outline";
                break;
            case ISnackbarColor.Error: 
                icon = "mdi-alert-outline";
                break;
            default:
                icon = "mdi-information-outline";
                break;
        }
    }
    return icon;
}
</script>