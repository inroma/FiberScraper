<template>
  <VApp>
    <VAppBar density="default" scroll-threshold="80" scroll-behavior="hide">
      <VAppBarNavIcon v-if="mdAndDown" @click.stop="drawer = !drawer"/>
      <VToolbarTitle align="left">Fiber Evolution Scraper</VToolbarTitle>
    </VAppBar>
    <v-navigation-drawer :model-value="drawer" expand-on-hover class="px-0 pt-0 nav-bar" rail permanent>
      <v-list>
        <v-list-item v-for="header in headers" link :to="header.url" :prepend-icon="header.icon"
        :title="header.title" :key="'header'+header.title" :disabled="header.disabled" density="default">
        </v-list-item>
      </v-list>
      <template #append>
        <v-list-item class="pl-10 pr-10">
          <v-btn @click="changeTheme()" color="primary">
            <v-icon>mdi-theme-light-dark</v-icon>
            Theme Switch
          </v-btn>
        </v-list-item>
      </template>
    </v-navigation-drawer>
    <VMain>
      <ToastComponent/>
      <router-view key="router-view" class="ma-5"/>
    </VMain>
  </VApp>
</template>

<script setup lang="ts">
import { ref } from 'vue';
import ToastComponent from './components/ToastComponent.vue';
import { useDisplay, useTheme } from 'vuetify/lib/framework.mjs';
import { watch } from 'vue';

const headers = [
	{
		title: 'Accueil',
		icon: 'mdi-home-outline',
		url: '/home',
		disabled: false
	},
	{
		title: 'Déploiements',
		icon: 'mdi-connection',
		url: '/',
		disabled: false
	},
	{
		title: 'Auto-Refresh',
		icon: 'mdi-timer-refresh-outline',
		url: '/auto-refresh',
		disabled: false
	},
	{
		title: 'About',
		icon: 'mdi-help-box',
		url: '/about',
		disabled: false
	},
];

const { mdAndDown } = useDisplay();
const { current } = useTheme();
const { global } = useTheme();

const drawer = ref(!mdAndDown.value);

watch(mdAndDown, () => drawer.value = !mdAndDown.value);

function changeTheme() {
	global.name.value = current.value.dark ? 'light' : 'dark';
}

</script>

<style>
#app {
  font-family: Avenir, Helvetica, Arial, sans-serif;
  -webkit-font-smoothing: antialiased;
  -moz-osx-font-smoothing: grayscale;
  text-align: center;
  color: #2c3e50;
}

.nav-bar {
  z-index: 1000 !important;
}

nav {
  padding: 30px;
}

nav a {
  font-weight: bold;
  color: #2c3e50;
}

nav a.router-link-exact-active {
  color: #42b983;
}
</style>