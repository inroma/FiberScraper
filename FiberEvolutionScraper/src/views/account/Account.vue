<script setup lang="ts">
import { ISnackbarColor } from '@/models/SnackbarInterface';
import router from '@/router';
import { userService } from '@/services/UserService';
import { useToastStore } from '@/store/ToastStore';
import { useUserStore } from '@/store/userStore';
import { computedAsync, useConfirmDialog } from '@vueuse/core';

const userStore = useUserStore();
const toastStore = useToastStore();
const showConfirmDialog = ref(false);

const userImage = computedAsync(async() => (await userStore.getUser())?.profile?.picture);

function deleteAccount() {
  userService.deleteAccount()
  .then(res => {
    if (res.data) {
      toastStore.createToastMessage({ message: 'Compte supprimé avec succès', color: ISnackbarColor.Success });
    } else {
      toastStore.createToastMessage({ message: 'Erreur pendant la suppression du compte', color: ISnackbarColor.Error });
    }
  })
  .finally(() => router.push('/'));
}

</script>
<template>
  <VDialog v-model="showConfirmDialog" width="fit-content">
    <VCard>
      <VCardTitle>
        <VIcon start color="red" icon="mdi-delete-forever" />Confirmer la suppression
      </VCardTitle>
      <VCardText>Voulez-vous vraiment supprimer votre compte ?</VCardText>
      <VCardActions>
          <VBtn class="mr-3" text="Confirmer" color="success" variant="outlined" @click="deleteAccount()"/>
          <VBtn text="Annuler" color="grey" variant="outlined" @click="() => showConfirmDialog = false"/>
      </VCardActions>
    </VCard>
  </VDialog>
  <VCard>
    <VCardText>
      <VRow align="center">
        <VCol class="d-flex align-center">
          <VAvatar v-if="userImage" :image="userImage" size="60" />
          <VIcon v-else icon="mdi-account-outline" size="60" />
          <span class="ml-5 font-weight-bold text-h5">{{ userStore.user?.userName }}</span>
        </VCol>
        <VCol class="d-flex justify-end">
          <VBtn text="Se déconnecter" color="grey" variant="outlined" @click="userStore.logout()"/>
          <VBtn class="ml-5" text="Supprimer le compte" color="red" variant="tonal" @click="showConfirmDialog = true"/>
        </VCol>
      </VRow>
      <VRow class="d-flex">
        <VCol>
          <span class="float-left">Inscription: {{ new Date(userStore.user?.created)?.toLocaleDateString() }}</span>
        </VCol>
      </VRow>
    </VCardText>
  </VCard>
</template>