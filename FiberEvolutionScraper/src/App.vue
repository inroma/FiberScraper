<template>
  <VApp>
    <VAppBar density="default" scroll-threshold="80" scroll-behavior="hide">
      <VAppBarNavIcon v-if="smAndDown" @click.stop="drawer = !drawer"/>
      <VToolbarTitle align="left" text="Fiber Evolution Scraper" />
      <template #append>
        <VBtn v-if="!isLoggedIn" text="Connexion" to="/auth/login"/>
      </template>
    </VAppBar>
    <VNavigationDrawer v-model:model-value="drawer" expand-on-hover class="px-0 pt-0 nav-bar" :rail="mdAndUp" permanent
      @update:rail="(v) => isExpanded = !v">
      <VList>
        <VListItem v-for="header in headers" link :to="header.url" :prepend-icon="header.icon"
        :title="header.title" :key="'header'+header.title" :disabled="header.disabled" density="default">
        </VListItem>
      </VList>
      <template #append>
        <VFadeTransition>
          <VBtn v-if="isExpanded || smAndDown" @click="changeTheme()">
            <VIcon icon="mdi-theme-light-dark"/>
          </VBtn>
        </VFadeTransition>
      </template>
    </VNavigationDrawer>
    <VMain>
      <ToastComponent/>
      <RouterView key="router-view" class="ma-5"/>
    </VMain>
  </VApp>
</template>

<script setup lang="ts">
import ToastComponent from './components/ToastComponent.vue';
import { useDisplay, useTheme } from 'vuetify';
import { auth } from './shared/services/auth/OAuthService';

const headers = [
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
const isExpanded = ref(false);
const drawer = ref(!smAndDown.value);

watchEffect(() => drawer.value = !smAndDown.value);

const isLoggedIn = computed(() => auth().isConnected);

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