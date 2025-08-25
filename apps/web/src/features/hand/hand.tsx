import React from 'react';

interface HandProps {
  cards: string[];
}

export function Hand({ cards }: HandProps) {
  return (
    <div className="flex gap-2">
      {cards.map((card) => (
        <div key={card} className="p-2 border rounded">
          {card}
        </div>
      ))}
    </div>
  );
}
