import { api } from "@fns/api";
import { UseQueryResult, useMutation, useQuery, useQueryClient } from "@tanstack/react-query";

export type ApiKeySummary = {
  id: string;
  name: string;
  keyPrefix: string;
  lastUsedAt: string | null;
  expiresAt: string | null;
  createdAt: string;
};

export type ApiKeyCreated = ApiKeySummary & {
  key: string;
};

const queryKey = ["api-keys"];

export function useApiKeys(): UseQueryResult<ApiKeySummary[]> {
  return useQuery({ queryKey, queryFn: () => api.get<ApiKeySummary[]>(`/v0/api-keys`) });
}

export function useCreateApiKey() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (name: string) => api.post<ApiKeyCreated>(`/v0/api-keys`, { name }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey });
    },
  });
}

export function useDeleteApiKey() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: string) => api.delete(`/v0/api-keys/${id}`),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey });
    },
  });
}
