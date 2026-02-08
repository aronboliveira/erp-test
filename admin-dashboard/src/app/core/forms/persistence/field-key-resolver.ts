export class FieldKeyResolver {
  static keyOf(el: Element): string | null {
    const dataId = el.getAttribute('data-id');
    if (dataId) return dataId;

    const id = el.getAttribute('id');
    if (id) return id;

    const name = el.getAttribute('name');
    return name || null;
  }

  static isWritable(el: Element): el is HTMLInputElement | HTMLTextAreaElement | HTMLSelectElement {
    return (
      el instanceof HTMLInputElement ||
      el instanceof HTMLTextAreaElement ||
      el instanceof HTMLSelectElement
    );
  }
}
