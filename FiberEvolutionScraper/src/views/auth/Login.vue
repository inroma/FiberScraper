<script setup lang="ts">
import { useUserStore } from '@/store/userStore';

const userStore = useUserStore();

onMounted(async () => {
  const user = await userStore.getUser();
  if (!user) {
    await userStore.signInRedirect();
  } else {
    await userStore.renewToken();
  }
});

</script>
<template>
  <VDialog persistent no-click-animation :scrim="false" :model-value="true" location="center" width="auto">
    <VCard class="px-10 py-3">
      Connexion en cours...
    </VCard>
  </VDialog>
</template>