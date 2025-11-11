<template>
  <VNavigationDrawer v-model="drawer" expand-on-hover class="px-0 py-0 nav-bar"
    @update:rail="(v) => isExpanded = !v" :rail="mdAndUp" :temporary="smAndDown" mobile-breakpoint="sm">
    <VList>
      <VListItem v-for="header of headers" link :to="header.url" :prepend-icon="header.icon"
        :title="header.title" :key="'header'+header.title" :disabled="header.disabled" density="default">
      </VListItem>
    </VList>
    <template #append>
      <VFadeTransition>
        <VRow v-if="isExpanded || smAndDown" class="flex-column">
          <VCol v-if="userStore.isConnected">
            <VBtn @click="userStore.logout()" height="50" prepend-icon="mdi-power-standby">Déconnexion</VBtn>
          </VCol>
          <VCol>
            <VBtn @click="changeTheme()" height="50" flat>
              <VIcon icon="mdi-theme-light-dark"/>
            </VBtn>
          </VCol>
        </VRow>
      </VFadeTransition>
    </template>
  </VNavigationDrawer>
</template>
<script setup lang="ts">
import { useUserStore } from '@/store/userStore';
import { useDisplay, useTheme } from 'vuetify';


const { smAndDown, mdAndUp } = useDisplay();
const theme = useTheme();
const isExpanded = ref(false);
const userStore = useUserStore();

const drawer = defineModel<boolean>("drawer");

const headers = computed(() => [
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
		disabled: userStore.isConnected !== true
	},
	{
		title: 'Compte',
		icon: 'mdi-account',
		url: '/account',
		disabled: userStore.isConnected !== true
	}
]);

// watchEffect(() => drawer.value = !smAndDown.value);

function changeTheme() {
  theme.change(theme.current.value.dark ? 'light' : 'dark');
}

</script>