<template>
  <VApp>
    <VAppBar density="default" scroll-threshold="80" scroll-behavior="hide">
      <VAppBarNavIcon v-if="smAndDown" @click.stop="drawer = !drawer"/>
      <VToolbarTitle align="left" text="Fiber Evolution Scraper" />
    </VAppBar>
    <VNavigationDrawer :model-value="drawer" expand-on-hover class="px-0 pt-0 nav-bar" :rail="mdAndUp" permanent>
      <VList>
        <VListItem v-for="header in headers" link :to="header.url" :prepend-icon="header.icon"
        :title="header.title" :key="'header'+header.title" :disabled="header.disabled" density="default">
        </VListItem>
      </VList>
      <template #append>
        <VListItem class="pl-10 pr-10">
          <VBtn @click="changeTheme()" color="primary" text="Theme Switch" #prepend>
            <VIcon icon="mdi-theme-light-dark"/>
          </VBtn>
        </VListItem>
      </template>
    </VNavigationDrawer>
    <VMain>
      <ToastComponent/>
      <RouterView key="router-view" class="ma-5"/>
    </VMain>
  </VApp>
</template>

<script setup lang="ts">
import { ref, watchEffect } from 'vue';
import ToastComponent from './components/ToastComponent.vue';
import { useDisplay, useTheme } from 'vuetify';

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
	}
];

const { smAndDown, mdAndUp } = useDisplay();
const { current, global } = useTheme();

const drawer = ref(!smAndDown.value);

watchEffect(() => drawer.value = !smAndDown.value);

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