interface ActionSpotProps {
  label: string;
}

export function ActionSpot({ label }: ActionSpotProps) {
  return <div className="px-2 py-1 border rounded text-xs">{label}</div>;
}
