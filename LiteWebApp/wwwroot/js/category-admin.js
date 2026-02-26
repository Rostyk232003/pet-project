// Ініціалізація модалок Bootstrap
const catModal = new bootstrap.Modal(document.getElementById('categoryModal'));
const delModal = new bootstrap.Modal(document.getElementById('deleteCategoryModal'));

/**
 * Відкриває модалку для створення або редагування
 * @param {string} id - Guid категорії (порожньо для нової)
 * @param {string} name - Назва категорії
 */
function openCategoryModal(id, name) {
    // Очищаємо або заповнюємо поля
    document.getElementById('modalCategoryId').value = id;
    document.getElementById('modalCategoryName').value = name;

    // Змінюємо заголовок залежно від дії
    document.getElementById('categoryModalTitle').innerText = id ? 'Редагувати категорію' : 'Додати категорію';

    catModal.show();
}

/**
 * Відкриває модалку підтвердження видалення
 */
function openDeleteModal(id, name) {
    document.getElementById('deleteCategoryId').value = id;
    document.getElementById('deleteCategoryNameDisplay').innerText = name;
    delModal.show();
}