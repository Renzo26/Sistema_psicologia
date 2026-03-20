import { computed } from '@angular/core';
import { signalStore, withState, withComputed, withMethods, patchState } from '@ngrx/signals';

export interface AuthUser {
  id: string;
  nome: string;
  email: string;
  role: 'admin' | 'gerente' | 'secretaria' | 'psicologo';
  clinicaId: string;
}

interface AuthState {
  user: AuthUser | null;
  accessToken: string | null;
  loading: boolean;
}

const initialState: AuthState = {
  user: null,
  accessToken: null,
  loading: false,
};

export const AuthStore = signalStore(
  { providedIn: 'root' },
  withState(initialState),
  withComputed((store) => ({
    isAuthenticated: computed(() => !!store.accessToken()),
    userRole: computed(() => store.user()?.role ?? null),
    userName: computed(() => store.user()?.nome ?? ''),
    clinicaId: computed(() => store.user()?.clinicaId ?? null),
  })),
  withMethods((store) => ({
    setAccessToken(token: string): void {
      patchState(store, { accessToken: token });
    },
    loginSuccess(user: AuthUser, accessToken: string): void {
      patchState(store, { user, accessToken, loading: false });
    },
    setLoading(loading: boolean): void {
      patchState(store, { loading });
    },
    logout(): void {
      patchState(store, { user: null, accessToken: null, loading: false });
    },
  }))
);
