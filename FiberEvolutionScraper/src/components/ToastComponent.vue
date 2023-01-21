<template>
    <div name="snackbars">
        <v-snackbar v-model="show" :color="color" :timeout="timeout" :top="'top'">
        {{ text }}

        <template v-slot:action="{ attrs }">
            <v-btn dark text v-bind="attrs" @click="show = false">
            Close
            </v-btn>
        </template>
        </v-snackbar>
    </div>
</template>
<script lang="ts" src="./ToastComponent.ts"/>