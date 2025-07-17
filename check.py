import requests
import json
import sys
from typing import Dict, Any, Optional

class WorkflowTester:
    def __init__(self, base_url: str = "http://localhost:5000"):
        self.base_url = base_url.rstrip('/')
        self.definition_id: Optional[str] = None
        self.instance_id: Optional[str] = None
        self.passed = 0
        self.failed = 0
        
    def test(self, name: str, condition: bool, message: str = ""):
        """Simple test assertion with logging"""
        if condition:
            print(f"✅ {name}")
            self.passed += 1
        else:
            print(f"❌ {name}: {message}")
            self.failed += 1
        return condition
    
    def request(self, method: str, endpoint: str, data: Dict[Any, Any] = None) -> tuple[bool, Dict[Any, Any]]:
        """Make HTTP request and return success status and response"""
        try:
            url = f"{self.base_url}{endpoint}"
            response = getattr(requests, method.lower())(url, json=data, timeout=10)
            return response.status_code < 400, response.json() if response.text else {}
        except Exception as e:
            return False, {"error": str(e)}
    
    def test_definitions(self):
        """Test workflow definition operations"""
        print("\n Testing Definitions...")
        
        # Create definition
        definition = {
            "name": "Test Workflow",
            "states": [
                {"id": "pending", "name": "Pending", "isInitial": True, "isFinal": False, "enabled": True},
                {"id": "approved", "name": "Approved", "isInitial": False, "isFinal": False, "enabled": True},
                {"id": "completed", "name": "Completed", "isInitial": False, "isFinal": True, "enabled": True}
            ],
            "actions": [
                {"id": "approve", "name": "Approve", "enabled": True, "fromStates": ["pending"], "toState": "approved"},
                {"id": "complete", "name": "Complete", "enabled": True, "fromStates": ["approved"], "toState": "completed"}
            ]
        }
        
        success, response = self.request('post', '/api/definitions', definition)
        if self.test("Create definition", success and response.get('success')):
            self.definition_id = response.get('data', {}).get('id')
        
        # Get definition
        success, response = self.request('get', f'/api/definitions/{self.definition_id}')
        self.test("Get definition", success and response.get('success'))
        
        # Get all definitions
        success, response = self.request('get', '/api/definitions')
        self.test("Get all definitions", success and response.get('success'))
    
    def test_instances(self):
        """Test workflow instance operations"""
        print("\n🔄 Testing Instances...")
        
        if not self.definition_id:
            print("  Skipping instance tests - no definition ID")
            return
        
        # Start instance
        success, response = self.request('post', f'/api/instances/{self.definition_id}')
        if self.test("Start instance", success and response.get('success')):
            self.instance_id = response.get('data', {}).get('id')
        
        # Get instance
        success, response = self.request('get', f'/api/instances/{self.instance_id}')
        self.test("Get instance", success and response.get('success'))
        
        # Get all instances
        success, response = self.request('get', '/api/instances')
        self.test("Get all instances", success and response.get('success'))
    
    def test_actions(self):
        """Test workflow action execution"""
        print("\n Testing Actions...")
        
        if not self.instance_id:
            print("  Skipping action tests - no instance ID")
            return
        
        # Execute approve action
        success, response = self.request('post', f'/api/instances/{self.instance_id}/actions', {"actionId": "approve"})
        self.test("Execute approve action", success and response.get('success'))
        
        # Execute complete action
        success, response = self.request('post', f'/api/instances/{self.instance_id}/actions', {"actionId": "complete"})
        self.test("Execute complete action", success and response.get('success'))
        
        # Try invalid action (should fail)
        success, response = self.request('post', f'/api/instances/{self.instance_id}/actions', {"actionId": "invalid"})
        self.test("Reject invalid action", not success or not response.get('success'))
    
    def test_validation(self):
        """Test validation scenarios"""
        print("\n🔍 Testing Validation...")
        
        # Invalid definition (no initial state)
        invalid_def = {
            "name": "Invalid",
            "states": [{"id": "test", "name": "Test", "isInitial": False, "isFinal": False, "enabled": True}],
            "actions": []
        }
        success, response = self.request('post', '/api/definitions', invalid_def)
        self.test("Reject invalid definition", not success or not response.get('success'))
        
        # Non-existent definition
        success, response = self.request('get', '/api/definitions/nonexistent')
        self.test("Handle non-existent definition", not success or not response.get('success'))
        
        # Non-existent instance
        success, response = self.request('get', '/api/instances/nonexistent')
        self.test("Handle non-existent instance", not success or not response.get('success'))
    
    def run_all_tests(self):
        """Run all tests and generate report"""
        print(" Workflow Engine Test Suite")
        print("=" * 40)
        
        # Check server connectivity
        success, _ = self.request('get', '/api/definitions')
        if not self.test("Server connectivity", success):
            print("\n❌ Server unreachable. Ensure workflow engine is running.")
            return
        
        # Run all test suites
        self.test_definitions()
        self.test_instances()
        self.test_actions()
        self.test_validation()
        
        # Generate summary
        total = self.passed + self.failed
        success_rate = (self.passed / total * 100) if total > 0 else 0
        
        print(f"\n{'='*40}")
        print(f" Results: {self.passed}/{total} passed ({success_rate:.1f}%)")
        print(f"Status: {'✅ PASS' if self.failed == 0 else '❌ FAIL'}")
        
        if self.failed > 0:
            print(f"❌ {self.failed} test(s) failed")

if __name__ == "__main__":
    base_url = sys.argv[1] if len(sys.argv) > 1 else "http://localhost:5000"
    tester = WorkflowTester(base_url)
    tester.run_all_tests()
