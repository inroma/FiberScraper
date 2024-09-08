<style scoped>
</style>
<template>
    <VSnackbar v-for="(snackbar, i) in toastStore.getToasts" :key="i"
    :model-value="snackbar.show" :color="snackbar.color" :timeout="snackbar.timeout" timer="white">
        <VIcon v-if="getIcon(snackbar)" class="pr-2">{{ getIcon(snackbar) }}</VIcon>

        {{ snackbar.message }}

        <template #actions>
            <VBtn @click="hideToast(snackbar)" icon :absolute="snackbar.message.length > 300 ? true : false" top right>
                <VIcon>mdi-window-close</VIcon>
            </VBtn>
        </template>
    </VSnackbar>
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