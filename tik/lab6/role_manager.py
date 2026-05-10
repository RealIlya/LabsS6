# Система ролевого управления доступом (RBAC)


class Role:
    # Роли с различными привилегиями (принцип минимизации привилегий)
    
    ROLES = {
        'admin': {
            'description': 'Администратор',
            'permissions': [
                'view_data',
                'edit_data',
                'delete_data',
                'create_data',
                'manage_users',
                'view_logs',
                'export_data'
            ]
        },
        'editor': {
            'description': 'Редактор',
            'permissions': [
                'view_data',
                'edit_data',
                'create_data'
            ]
        },
        'viewer': {
            'description': 'Просмотрщик',
            'permissions': [
                'view_data'
            ]
        }
    }
    
    @staticmethod
    def has_permission(role, permission):
        # Проверка наличия прав доступа
        if role not in Role.ROLES:
            return False
        return permission in Role.ROLES[role]['permissions']
    
    @staticmethod
    def get_role_description(role):
        # Получить описание роли
        return Role.ROLES.get(role, {}).get('description', 'Неизвестная роль')
    
    @staticmethod
    def can_edit_record(user_id, user_role, record_user_id):
        # Проверка прав на редактирование конкретной записи
        # - Admin может редактировать всё
        # - Editor может редактировать только свои записи
        # - Viewer не может редактировать
        if not Role.has_permission(user_role, 'edit_data'):
            return False
        
        if user_role == 'admin':
            return True
        
        if user_role == 'editor':
            return user_id == record_user_id
        
        return False
    
    @staticmethod
    def can_delete_record(user_id, user_role, record_user_id):
        # Проверка прав на удаление конкретной записи
        # - Admin может удалять всё
        # - Остальные не могут удалять
        if not Role.has_permission(user_role, 'delete_data'):
            return False
        
        if user_role == 'admin':
            return True
        
        return False