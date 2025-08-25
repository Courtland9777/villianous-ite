import { render, screen } from '@testing-library/react';
import { describe, expect, it } from 'vitest';
import { Hand } from './hand';

describe('Hand', () => {
  it('renders provided card names', () => {
    render(<Hand cards={['Card A', 'Card B']} />);
    expect(screen.getByText('Card A')).toBeInTheDocument();
    expect(screen.getByText('Card B')).toBeInTheDocument();
  });
});
