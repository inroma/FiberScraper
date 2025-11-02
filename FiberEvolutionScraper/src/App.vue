<template>
  <VApp>
    <VAppBar density="default" scroll-threshold="80" scroll-behavior="hide">
      <VAppBarNavIcon v-if="smAndDown" @click.stop="drawer = !drawer"/>
      <VToolbarTitle align="left" :text="appTitle" />
      <template #append>
        <VBtn v-if="!userStore.isConnected" text="Connexion" to="/auth/login" :loading="userStore.loginLoading"/>
      </template>
    </VAppBar>
    <NavigationDrawer v-model:drawer="drawer" key="nav-drawer" />
    <VMain>
      <ToastComponent/>
      <RouterView :key="router.currentRoute.value.path"/>
    </VMain>
  </VApp>
</template>
<script setup lang="ts">
import ToastComponent from './components/ToastComponent.vue';
import { useDisplay } from 'vuetify';
import router from './router';
import { useUserStore } from './store/userStore';

const { smAndDown } = useDisplay();
const drawer = ref(!smAndDown.value);
const appTitle = import.meta.env.VITE_APP_TITLE;
const userStore = useUserStore();

watchEffect(() => drawer.value = !smAndDown.value);

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