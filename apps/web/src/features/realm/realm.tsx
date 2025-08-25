import React from 'react';

interface Location {
  id: string;
  name: string;
}

interface RealmProps {
  locations: Location[];
}

export function Realm({ locations }: RealmProps) {
  return (
    <div className="flex gap-4">
      {locations.map((loc) => (
        <div key={loc.id} className="p-4 border rounded">
          {loc.name}
        </div>
      ))}
    </div>
  );
}
