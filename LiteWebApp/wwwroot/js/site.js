function addCharacteristic() {
    const container = document.getElementById('characteristics-container');
    const index = container.querySelectorAll('.char-row').length;

    const html = `
        <div class="row g-2 mb-2 char-row">
            <div class="col-5">
                <input name="Characteristics[${index}].Key" class="form-control form-control-sm" placeholder="Назва" />
            </div>
            <div class="col-6">
                <input name="Characteristics[${index}].Value" class="form-control form-control-sm" placeholder="Значення" />
            </div>
            <div class="col-1 text-end">
                <button type="button" class="btn btn-sm btn-outline-danger w-100" onclick="removeRow(this)">
                    <i class="bi bi-trash"></i>
                </button>
            </div>
        </div>
    `;
    container.insertAdjacentHTML('beforeend', html);
}

function removeRow(button) {
    const row = button.closest('.char-row');
    row.remove();
    reIndexCharacteristics(); // Важливо: перенумеровуємо індекси після видалення
}

function reIndexCharacteristics() {
    const rows = document.querySelectorAll('.char-row');
    rows.forEach((row, index) => {
        const inputs = row.querySelectorAll('input');
        inputs.forEach(input => {
            const name = input.getAttribute('name');
            if (name.includes('Key')) {
                input.setAttribute('name', `Characteristics[${index}].Key`);
            } else {
                input.setAttribute('name', `Characteristics[${index}].Value`);
            }
        });
    });
}
