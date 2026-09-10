import { Alert, AlertDescription, AlertTitle } from "@components/Alert";
import { Button } from "@components/Button";
import { ErrorState } from "@components/ErrorState";
import { LoadingState } from "@components/LoadingState";
import { Page, PageHeading } from "@components/Page";
import { TextInput } from "@components/TextInput";
import { IconAlertTriangle, IconKey } from "@tabler/icons-react";
import { useState } from "react";
import { toast } from "sonner";
import { ApiKeyListItem } from "./ApiKeyListItem";
import { ApiKeyCreated, ApiKeySummary, useApiKeys, useCreateApiKey, useDeleteApiKey } from "./useApiKeys";

Component.displayName = "ApiKeysPage";
export function Component() {
  const { isLoading, isError, data: apiKeys, refetch } = useApiKeys();

  return (
    <Page title="API Keys">
      <PageHeading title="API Keys" subtitle="Manage API keys for programmatic access to your account" />

      <div className="max-w-3xl mt-8 space-y-8">
        {isLoading && <LoadingState />}
        {isError && <ErrorState refetch={refetch} />}
        {apiKeys && <Body apiKeys={apiKeys} />}
      </div>
    </Page>
  );
}

function Body(props: { apiKeys: ApiKeySummary[] }) {
  const [name, setName] = useState("");
  const [justCreated, setJustCreated] = useState<ApiKeyCreated | null>(null);
  const [revokingId, setRevokingId] = useState<string | null>(null);

  const createMutation = useCreateApiKey();
  const deleteMutation = useDeleteApiKey();

  const handleCreate = async (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    if (!name.trim()) return;

    try {
      const created = await createMutation.mutateAsync(name.trim());
      setJustCreated(created);
      setName("");
    } catch {
      toast.error("Failed to create API key");
    }
  };

  const handleRevoke = async (id: string) => {
    setRevokingId(id);
    try {
      await deleteMutation.mutateAsync(id);
      toast.success("API key revoked");
    } catch {
      toast.error("Failed to revoke API key");
    } finally {
      setRevokingId(null);
    }
  };

  return (
    <>
      <Alert variant="warning">
        <IconAlertTriangle className="h-4 w-4" />
        <AlertTitle>API keys grant FULL access to your account</AlertTitle>
        <AlertDescription className="text-muted-foreground">
          A key is equivalent to being signed in: anyone holding it can do anything you can, including deleting your
          account and accessing billing, in addition to creating, modifying, or deleting apps and shares. There is no
          way to scope a key down. Treat keys like passwords and store them somewhere safe.
        </AlertDescription>
      </Alert>

      {justCreated && (
        <Alert variant="warning">
          <IconKey className="h-4 w-4" />
          <AlertTitle>Copy this key now &mdash; it will not be shown again</AlertTitle>
          <AlertDescription>
            <code className="block bg-muted px-2 py-1.5 rounded mt-2 mb-3 break-all text-foreground">
              {justCreated.key}
            </code>
            <Button size="sm" variant="secondary" onClick={() => setJustCreated(null)}>
              Done, I've saved it
            </Button>
          </AlertDescription>
        </Alert>
      )}

      <div>
        <div className="flex items-center space-x-2 mb-4">
          <IconKey className="h-5 w-5 text-foreground" />
          <h3 className="text-lg font-medium">Your API Keys</h3>
        </div>

        {props.apiKeys.length >= 1 && (
          <ul className="space-y-2 mb-6">
            {props.apiKeys.map((key) => (
              <ApiKeyListItem key={key.id} apiKey={key} onRevoke={handleRevoke} revoking={revokingId === key.id} />
            ))}
          </ul>
        )}
        {props.apiKeys.length === 0 && (
          <p className="text-sm text-muted-foreground mb-6">You don't have any API keys yet.</p>
        )}

        <form onSubmit={handleCreate} className="space-y-4 max-w-[40rem]">
          <div className="flex items-center space-x-2">
            <TextInput
              label="Create a new key:"
              name="name"
              required={true}
              value={name}
              placeholder="e.g. terraform-ci"
              maxLength={100}
              onChange={(e) => setName(e.target.value)}
              description="Give it a name that helps you identify where it's used."
            />
            <Button disabled={name.trim().length === 0} loading={createMutation.isPending}>
              Create key
            </Button>
          </div>
        </form>
      </div>
    </>
  );
}
