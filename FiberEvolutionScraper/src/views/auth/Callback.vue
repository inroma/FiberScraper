<script setup lang="ts">
import router from '@/router';
import { useUserStore } from '@/store/userStore';

const userStore = useUserStore();

onMounted(async () => {
	await userStore.signInCallback();
	router.push("/");
});

</script>
<template>
  <VDialog persistent no-click-animation :scrim="false" :model-value="true" location="center" width="auto">
    <VCard class="px-10 py-3">
      Connexion en cours...
    </VCard>
  </VDialog>
</template>