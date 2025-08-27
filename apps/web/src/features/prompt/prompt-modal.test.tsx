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

    const dialog = screen.getByRole('dialog');
    const button = screen.getByRole('button', { name: /close/i });

    expect(dialog).toHaveTextContent('Hello there');
    expect(button).toHaveFocus();

    fireEvent.keyDown(dialog, { key: 'Tab' });
    expect(button).toHaveFocus();

    fireEvent.keyDown(dialog, { key: 'Escape' });
    expect(screen.queryByRole('dialog')).not.toBeInTheDocument();
  });
});
