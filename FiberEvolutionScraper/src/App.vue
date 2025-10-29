<template>
  <VApp>
    <VAppBar density="default" scroll-threshold="80" scroll-behavior="hide">
      <VAppBarNavIcon v-if="smAndDown" @click.stop="drawer = !drawer"/>
      <VToolbarTitle align="left" :text="appTitle" />
      <template #append>
        <VBtn v-if="!loggedIn" text="Connexion" to="/auth/login"/>
      </template>
    </VAppBar>
    <NavigationDrawer v-model:drawer="drawer" />
    <VMain>
      <ToastComponent/>
      <RouterView key="router-view" class="ma-5"/>
    </VMain>
  </VApp>
</template>
<script setup lang="ts">
import ToastComponent from './components/ToastComponent.vue';
import { useDisplay } from 'vuetify';
import { useAuth } from '@/shared/composables/OAuthComposable';

const { smAndDown } = useDisplay();
const drawer = ref(!smAndDown.value);
const appTitle = import.meta.env.VITE_APP_TITLE;
const auth = useAuth();

watchEffect(() => drawer.value = !smAndDown.value);

const loggedIn = computed(() => auth?.isConnected.value);

</script>
<style>
#app {
  font-family: Avenir, Helvetica, Arial, sans-serif;
  -webkit-font-smoothing: antialiased;
  -moz-osx-font-smoothing: grayscale;
  text-align: center;
  color: #2c3e50;
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