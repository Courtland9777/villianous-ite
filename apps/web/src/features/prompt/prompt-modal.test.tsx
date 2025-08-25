import { render, screen, fireEvent } from '@testing-library/react';
import { describe, expect, it, beforeEach } from 'vitest';
import { PromptModal } from './prompt-modal';
import { usePromptsStore } from '../../stores/prompts.store';

describe('PromptModal', () => {
  beforeEach(() => {
    usePromptsStore.setState({ prompt: null });
  });

  it('displays current prompt and can be closed', () => {
    usePromptsStore.getState().showPrompt({ id: '1', message: 'Hello there' });
    render(<PromptModal />);

    expect(screen.getByRole('dialog')).toHaveTextContent('Hello there');

    fireEvent.click(screen.getByRole('button', { name: /close/i }));
    expect(screen.queryByRole('dialog')).not.toBeInTheDocument();
  });
});
