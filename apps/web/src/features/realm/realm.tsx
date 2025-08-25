import React from 'react';
import { ActionSpot } from './action-spot';

interface ActionSpot {
  id: string;
  label: string;
}

interface Location {
  id: string;
  name: string;
  spots: ActionSpot[];
}

interface RealmProps {
  locations: Location[];
}

export function Realm({ locations }: RealmProps) {
  return (
    <div className="flex gap-4">
      {locations.map((loc) => (
        <div key={loc.id} className="p-4 border rounded space-y-2">
          <div>{loc.name}</div>
          <div className="flex flex-col gap-1">
            {loc.spots.map((spot) => (
              <ActionSpot key={spot.id} label={spot.label} />
            ))}
          </div>
        </div>
      ))}
    </div>
  );
}
