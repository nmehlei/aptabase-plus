import { Button } from "@components/Button";
import { formatDate } from "@fns/format-date";
import { IconTrash } from "@tabler/icons-react";
import { ApiKeySummary } from "./useApiKeys";

type Props = {
  apiKey: ApiKeySummary;
  onRevoke: (id: string) => void;
  revoking: boolean;
};

export function ApiKeyListItem(props: Props) {
  const { apiKey } = props;

  return (
    <li className="flex items-center justify-between bg-card border px-4 py-3 rounded-md gap-4">
      <div className="truncate">
        <p className="font-medium truncate">{apiKey.name}</p>
        <p className="text-xs text-muted-foreground mt-1 space-x-3">
          <code className="bg-muted px-1 py-0.5 rounded">{apiKey.keyPrefix}&hellip;</code>
          <span>Created {formatDate(apiKey.createdAt)}</span>
          <span>Last used {apiKey.lastUsedAt ? formatDate(apiKey.lastUsedAt) : "never"}</span>
          <span>Expires {apiKey.expiresAt ? formatDate(apiKey.expiresAt) : "never"}</span>
        </p>
      </div>
      <Button
        variant="ghost"
        size="sm"
        disabled={props.revoking}
        loading={props.revoking}
        onClick={() => props.onRevoke(apiKey.id)}
      >
        <IconTrash className="w-4 h-4 mr-1" />
        Revoke
      </Button>
    </li>
  );
}
